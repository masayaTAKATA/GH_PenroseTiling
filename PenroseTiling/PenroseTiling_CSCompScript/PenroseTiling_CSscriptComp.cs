using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(int num, double length, ref object PenroseLines)
  {
    
    //Declare start and rule string
    string finalString = "[7]++[7]++[7]++[7]++[7]";
    string rule6 = "81++91----71[-81----61]++";
    string rule7 = "+81--91[---61--71]+";
    string rule8 = "-61++71[+++81++91]-";
    string rule9 = "--81++++61[+91++++71]--71";

    //Generate the string
    GrowString(ref num, ref finalString, rule6, rule7, rule8, rule9);
    //Generate the points
    var penroseLines = new List<Line>();
    ParsepenroseString(finalString, length, ref penroseLines);

    //Assign output
    PenroseLines = penroseLines;

  }

  // <Custom additional code> 
    private void GrowString(ref int num, ref string finalString, string rule6, string rule7, string rule8, string rule9)
  {
    //Decrement the count with each new execution of the grow function
    num = num - 1;
    char rule;

    //Create new string
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

      if(rule == '[' || rule == ']' || rule == '+' || rule == '-')
      {
        newString = newString + rule;
      }
    }
    finalString = newString;

    //Stop condition
    if(num == 0){return;}

    //Grow agein(recursive)
    GrowString(ref num, ref finalString, rule6, rule7, rule8, rule9);
  }

  private void ParsepenroseString(string penroseString, double length, ref List<Line> penroseLines)
  {
    //Parse instruction string to generate points
    //Let base point be world origin
    var pt = Point3d.Origin;

    //Declare points list
    //Vector rotate with (+.-) instruction by 36degrees
    var listPts = new List<Point3d>();

    //Draw forward direction
    //Vector direction will be rotated depend on (+, -) instructions
    var vec = new Vector3d(1.0, 0.0, 0.0);

    //Stacks of points and vectors
    var ptStack = new List<Point3d>();
    var vStack = new List<Vector3d>();

    //Declare loop variables
    char rule;
    for(int i = 0; i < penroseString.Length; i++)
    {
      //Always start for 1 and length  1 to get one char at a time
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
      //draw forward by direction
      if(rule == '1')
      {
        //Add current points
        var newPt1 = new Point3d(pt);
        listPts.Add(newPt1);

        //Calculate next point
        var newPt2 = pt + (vec * length);
        //Add next point
        listPts.Add(newPt2);

        //Save new location
        pt = newPt2;
      }

      //Save point location
      if(rule == '[')
      {
        //Save current point and direction
        var newPt = new Point3d(pt);
        ptStack.Add(newPt);

        var newVec = new Vector3d(vec);
        vStack.Add(newVec);
      }
      //Retrieve point and directon
      if(rule == ']')
      {
        pt = ptStack[ptStack.Count - 1];
        vec = vStack[vStack.Count - 1];
        //Remove from stack
        ptStack.RemoveAt(ptStack.Count - 1);
        vStack.RemoveAt(vStack.Count - 1);
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

    Print(allLines.Count.ToString());


  }
  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        int num = default(int);
    if (inputs[0] != null)
    {
      num = (int)(inputs[0]);
    }

    double length = default(double);
    if (inputs[1] != null)
    {
      length = (double)(inputs[1]);
    }



    //3. Declare output parameters
      object PenroseLines = null;


    //4. Invoke RunScript
    RunScript(num, length, ref PenroseLines);
      
    try
    {
      //5. Assign output parameters to component...
            if (PenroseLines != null)
      {
        if (GH_Format.TreatAsCollection(PenroseLines))
        {
          IEnumerable __enum_PenroseLines = (IEnumerable)(PenroseLines);
          DA.SetDataList(1, __enum_PenroseLines);
        }
        else
        {
          if (PenroseLines is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(PenroseLines));
          }
          else
          {
            //assign direct
            DA.SetData(1, PenroseLines);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}