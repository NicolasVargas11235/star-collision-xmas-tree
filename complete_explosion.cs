using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


using System.Drawing;

/**
Program Name: Star Collision: Complete Explosion
Author: Nicolas Vargas
Date completed: 1/4/2022

Special Thanks to Jose Luis from Parametric Camp for providing public
lectures/tutorials on modeling a christmas tree using Rhino and Grasshopper.

Link the Parametric Camp youtube channel: https://www.youtube.com/channel/UCSgG9KzVsS6jArapCx-Bslg
**/


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
  private void RunScript(List<Point3d> leds, int fps, int duration, Point3d origin, Curve maxLength, double radius, Curve path, Curve path2, ref object CSV)
  {
    
    // Count how many LED lights are on the tree
    int ledCount = leds.Count;

    // Prepare the basics of the CSV file
    List<string> csv = new List<string>();

    // Add the first row of the CSV, which looks like:
    // "FRAME_ID,R_0,G_0,B_0,R_1,G_1,B_1..."
    string row = "FRAME_ID,";
    for (int i = 0; i < ledCount; i++)
    {
      row += "R_" + i + ",G_" + i + ",B_" + i;
      if (i != ledCount - 1)
      {
        row += ",";
      }
    }
    csv.Add(row);

    // Total length of animation
    int frameCount = fps * duration;

    // Define important values for torus explosion
    Point3d[] pts;
    maxLength.DivideByCount(frameCount, true, out pts);
    double fullLength = maxLength.GetLength();

    // Define important values for upper ray burst
    Point3d[] pts1;
    path.DivideByCount(frameCount, true, out pts1);
    double fullLength1 = path.GetLength();

    //Define important values for lower ray burst
    Point3d[] pts2;
    path2.DivideByCount(frameCount, true, out pts2);
    double fullLength2 = path2.GetLength();



    // Generate rows for each frame
    for (int f = 0; f < frameCount; f++)
    {
      row = f + ",";

      double currentLength = (pts[f].X);
      double currentLength1 = (pts1[0].DistanceTo(pts1[f]));
      double currentLength2 = (pts2[0].DistanceTo(pts2[f]));

      // Calculate the values for each LED
      int r, g, b;
      for (int i = 0; i < ledCount; i++)
      {
        // Calculate distance of LED from z axis and LED height from origin point
        double xyDistance = Math.Sqrt(Math.Pow(leds[i].X, 2) + Math.Pow(leds[i].Y, 2));
        double zDistance = Math.Sqrt(Math.Pow(xyDistance, 2) + Math.Pow((origin.Z - leds[i].Z), 2));

        //Calculate Radii of torus
        double radiusMinor = 1.85 * currentLength;
        double radiusMajor = 3.3 * currentLength;


        int hue = 210;
        double intensity = 1.0;


        // Is LED inside upper ray burst?
        if(xyDistance < radius && leds[i].Z < (pts1[f].Z * 1.85) && leds[i].Z > pts1[0].Z)
        {
          intensity = (double) (1 - (fullLength1 - currentLength1) / fullLength1);
        }

          // Is LED inside lower ray burst?
        else if(xyDistance < radius && leds[i].Z > pts2[f].Z * 1.85 && leds[i].Z < pts2[0].Z)
        {
          intensity = (double) (1 - (fullLength2 - currentLength2) / fullLength2);
        }

          // Is LED inside torus?
        else if(xyDistance < (radiusMajor + radiusMinor) && xyDistance > (radiusMajor - radiusMinor) &&
          (zDistance < radiusMinor))
        {
          intensity = (double) (1 - (fullLength - currentLength) / fullLength);
          hue = 390;
        }

        // Convert HSV to RGB colour for LED
        Color hueClr = ColorFromHSV(hue * (1 + intensity), intensity, 1 - intensity);

        r = hueClr.R;
        g = hueClr.G;
        b = hueClr.B;


        // Add these colors to the row
        row += r + "," + g + "," + b;
        if (i != ledCount - 1)
        {
          row += ",";
        }
      }

      // Add this row to the CSV
      csv.Add(row);
    }


    // Outputs
    CSV = csv;

  }

  // <Custom additional code> 
  
  // From https://stackoverflow.com/a/1626232/1934487
  public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
  {
    int max = Math.Max(color.R, Math.Max(color.G, color.B));
    int min = Math.Min(color.R, Math.Min(color.G, color.B));

    hue = color.GetHue();
    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
    value = max / 255d;
  }

  // From https://stackoverflow.com/a/1626232/1934487
  public static Color ColorFromHSV(double hue, double saturation, double value)
  {
    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    double f = hue / 60 - Math.Floor(hue / 60);

    value = value * 255;
    int v = Convert.ToInt32(value);
    int p = Convert.ToInt32(value * (1 - saturation));
    int q = Convert.ToInt32(value * (1 - f * saturation));
    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

    if (hi == 0)
      return Color.FromArgb(255, v, t, p);
    else if (hi == 1)
      return Color.FromArgb(255, q, v, p);
    else if (hi == 2)
      return Color.FromArgb(255, p, v, t);
    else if (hi == 3)
      return Color.FromArgb(255, p, q, v);
    else if (hi == 4)
      return Color.FromArgb(255, t, p, v);
    else
      return Color.FromArgb(255, v, p, q);
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
        List<Point3d> leds = null;
    if (inputs[0] != null)
    {
      leds = GH_DirtyCaster.CastToList<Point3d>(inputs[0]);
    }
    int fps = default(int);
    if (inputs[1] != null)
    {
      fps = (int)(inputs[1]);
    }

    int duration = default(int);
    if (inputs[2] != null)
    {
      duration = (int)(inputs[2]);
    }

    Point3d origin = default(Point3d);
    if (inputs[3] != null)
    {
      origin = (Point3d)(inputs[3]);
    }

    Curve maxLength = default(Curve);
    if (inputs[4] != null)
    {
      maxLength = (Curve)(inputs[4]);
    }

    double radius = default(double);
    if (inputs[5] != null)
    {
      radius = (double)(inputs[5]);
    }

    Curve path = default(Curve);
    if (inputs[6] != null)
    {
      path = (Curve)(inputs[6]);
    }

    Curve path2 = default(Curve);
    if (inputs[7] != null)
    {
      path2 = (Curve)(inputs[7]);
    }



    //3. Declare output parameters
      object CSV = null;


    //4. Invoke RunScript
    RunScript(leds, fps, duration, origin, maxLength, radius, path, path2, ref CSV);
      
    try
    {
      //5. Assign output parameters to component...
            if (CSV != null)
      {
        if (GH_Format.TreatAsCollection(CSV))
        {
          IEnumerable __enum_CSV = (IEnumerable)(CSV);
          DA.SetDataList(1, __enum_CSV);
        }
        else
        {
          if (CSV is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(CSV));
          }
          else
          {
            //assign direct
            DA.SetData(1, CSV);
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