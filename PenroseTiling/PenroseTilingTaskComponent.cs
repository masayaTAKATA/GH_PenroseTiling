using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PenroseTiling
{
    /// <summary>
    /// Penrose tiling diagram
    /// multi-threading component inherite from GH_TaskCapableComponent
    /// </summary>
    public class PenroseTilingTaskComponent : GH_TaskCapableComponent<PenroseTilingTaskComponent.SolveResults>
    {
        //Constructor
        public PenroseTilingTaskComponent() : base("PenroseTiling_Task", "PRT_t", "multi-threading compute Penrose diagram lines", "User", "Test")
        {
        }
        #region Input, Output
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number", "num", "Number of level", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Length", "length", "Line length", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "lines", "Penrose diagram lines", GH_ParamAccess.list);
        }
        #endregion

        public class SolveResults
        {
            public List<Line> Value { get; set; }
        }

        /// <summary>
        /// This is the method that create points list for recursive process
        /// Create a Compute function that takes the input retrieve from IGH_DataAccess
        /// and returns an instance of SolveResults
        /// </summary>
        private static SolveResults ComputePenroseLines(double length, int num)
        {
            //Declare start and rule strings (example)
            string finalString = "[7]++[7]++[7]++[7]++[7]";
            string rule6 = "81++91----71[-81----61]++";
            string rule7 = "+81--91[---61--71]+";
            string rule8 = "-61++71[+++81++91]-";
            string rule9 = "--81++++61[+91++++71]--71";

            var result = new SolveResults();

            var penroseString = finalString;
            GrowString(ref num, ref penroseString, rule6, rule7, rule8, rule9);

            var penroseLines = new List<Line>();
            ParsePenroseString(penroseString, length, ref penroseLines);

            result.Value = penroseLines;

            return result;
        }

        //Generate GrowString from rules
        private static void GrowString(ref int num, ref string finalString, string rule6, string rule7, string rule8, string rule9)
        {
            //Decrement the count with each new execution of the grow function.
            num = num - 1;
            char rule;

            //Create new string.
            string newString = "";
            for(int i = 0; i < finalString.Length; i++)
            {
                rule = finalString[i];
                if(rule == '6')
                {
                    newString = newString + rule6;
                }
                if(rule == '7')
                {
                    newString = newString + rule7;
                }
                if(rule == '8')
                {
                    newString = newString + rule8;
                }
                if(rule == '9')
                {
                    newString = newString + rule9;
                }
                if(rule =='[' || rule == ']' || rule == '+' || rule == '-')
                {
                    newString = newString + rule;
                }
            }
            finalString = newString;

            //Stop condition
            if(num == 0)
            {
                return;
            }

            //Grow again (recursive)
            GrowString(ref num, ref finalString, rule6, rule7, rule8, rule9);

        }

        //Parce PenroseString
        private static void ParsePenroseString(string penroseString, double length, ref List<Line>penroseLines)
        {
            //Parse instruction string to generate points
            //Let base point be world origin
            var pt = Point3d.Origin;

            //Declare points list
            //Vector rotate with (+, -) instruction by 36degrees.
            var listPts = new List<Point3d>();

            //Draw forward direction
            //Vector direction will be rotated depend on (+, -) instructions.
            var vec = new Vector3d(1.0, 0.0, 0.0);

            //Stacks of points and vectors
            var ptStack = new List<Point3d>();
            var vStack = new List<Vector3d>();

            //Declare loop variables
            char rule;
            for(int i = 0; i < penroseString.Length; i++)
            {
                //Always start for 1 and length 1 to get one char at a time.
                rule = penroseString[i];

                //rotate left
                if(rule == '+')
                {
                    vec.Rotate(36 * (Math.PI / 180), Vector3d.ZAxis);
                }
                //rotate right
                if(rule == '-')
                {
                    vec.Rotate(-36 * (Math.PI / 180), Vector3d.ZAxis);
                }
                //draw foward by direction
                if(rule == '1')
                {
                    //Add current points
                    var newPt1 = new Point3d(pt);
                    listPts.Add(newPt1);
                    //Caluculate next point
                    var newPt2 = pt + (vec * length);
                    //Add next point
                    listPts.Add(newPt2);
                    //Save new location
                    pt = newPt2;
                }
                //Save point location
                if(rule == '[')
                {
                    //Save current point ando direction
                    var newPt = new Point3d(pt);
                    ptStack.Add(newPt);

                    var newVec = new Vector3d(vec);
                    vStack.Add(newVec);
                }
                //Retrieve point and direction
                if(rule == ']')
                {
                    pt = ptStack[ptStack.Count - 1];
                    vec = vStack[vStack.Count - 1];
                    //Remove from stack
                    ptStack.RemoveAt(ptStack.Count - 1);
                    vStack.RemoveAt(vStack.Count -1);
                }
            }
            //Generate lines
            var allLines = new List<Line>();
            for(int i = 1; i < listPts.Count; i = i + 2)
            {
                var line = new Line(listPts[i - 1], listPts[i]);
                allLines.Add(line);
            }

            penroseLines = allLines;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare placeholder valuable for the input data
            double length = double.NaN;
            int num = 2;

            //Retrieve input data
            if(!DA.GetData(0, ref num)) { return; }
            if(!DA.GetData(1, ref length)) { return; }

            //we should now validate the data and warn the user if invalid data is supplied.
            if(num <= 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of depth must be bigger than One");
                return;
            }
            if(length < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Length must be bigger than Zero");
                return;
            }

            if (InPreSolve)
            {
                //Queue up the task
                Task<SolveResults> task = Task.Run(() => ComputePenroseLines(length, num));
                TaskList.Add(task);

                return;
            }

            if (!GetSolveResults(DA, out SolveResults result))
            {
                //Compute results on a given data
                result = ComputePenroseLines(length, num);
            }

            //Set output data
            if(result != null)
            {
                DA.SetDataList(0, result.Value);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

        public override Guid ComponentGuid => new Guid("{771289E0-5E53-4724-9EC8-DC020CDEA189}");
    }
}
