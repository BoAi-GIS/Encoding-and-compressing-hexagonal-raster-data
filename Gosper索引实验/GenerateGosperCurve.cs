using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;

namespace Compression
{
    /// <summary>
    /// Summary description for DrawGosperBaseMap.
    /// </summary>
    [Guid("7df5b714-571c-42ab-b877-bf65c94cf9ba")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.GenerateGosperCurve")]
    public sealed class GenerateGosperCurve : BaseCommand
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IApplication m_application;

        IPointCollection curvePtCol;//Set of Gosper curve nodes
        IPolyline GosperLine;//Gosper line result

        double oriX = 606463;//X coordinate of start point
        double oriY = 4270066;//Y coordinate of start point

        double GosperX;//X coordinate of moving point
        double GosperY;//Y coordinate of moving point

        double step = 100;//Step size
        double forwardAngle = Math.PI / 2;//Set initial forward forwardAngle  

        public GenerateGosperCurve()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text
            base.m_caption = "GenerateGosperCurve";  //localizable text
            base.m_message = "";  //localizable text 
            base.m_toolTip = "";  //localizable text 
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            //1 Gosper curve construction    
            //1.1 Generate character encoding
            int iterateNum = 3;//Set the number of iterations
            string codeString = "A";//Initial encoding

            //Iterative replacement of encoding by regular expression
            string replace = "A|B";
            Regex regex = new Regex(replace);
            MatchEvaluator matchEvaluator = new MatchEvaluator(Rules);
            //Complete the specified number of iterations
            for (int i = 0; i < iterateNum; i++)
            {
                codeString = regex.Replace(codeString, matchEvaluator);
            }
            ///1.2 Generate Gosper curves according to character encoding
            //Start point setting
            GosperX = oriX;
            GosperY = oriY;
            IPoint oriPt = new PointClass();
            oriPt.X = GosperX;
            oriPt.Y = GosperY;

            //Add start point to node set
            curvePtCol = new PolylineClass();
            curvePtCol.AddPoint(oriPt);

            //Translate character encoding and draw according to its meaning
            for (int i = 0; i < codeString.Length; i++)
            {
                string order = codeString[i].ToString();
                switch (order)
                {
                    case "A":
                    case "B":
                        GenerateGosperNode(forwardAngle);
                        break;
                    case "-":
                        forwardAngle -= Math.PI / 3;//Turn 60 degrees counter clockwise
                        break;
                    case "+":
                        forwardAngle += Math.PI / 3;//Turn 60 degrees clockwise
                        break;
                }
            }

            //2 Output the generated Gosper curve
            //Output address settings
            string gdbPath = @"FilePath\FileName.gdb";
            string GosperLineName = "GosperLine";//Set the output name
            CommonTool commonTool = new CommonTool(gdbPath);

            //Output the result
            GosperLine = curvePtCol as IPolyline;
            IFeatureClass gosperLineFeaClass = commonTool.CreateFeatureClassWithoutAttribute(GosperLineName, esriGeometryType.esriGeometryPolyline);
            IFeature lineFea = gosperLineFeaClass.CreateFeature();
            lineFea.Shape = GosperLine;
            lineFea.Store();

            MessageBox.Show("OK");
        }

        #endregion

        //Substitution rule 
        string Rules(Match match)
        {
            switch (match.Value)
            {
                case "A":
                    return "A-B--B+A++AA+B-";
                case "B":
                    return "+A-BB--B-A++A+B";
                default:
                    return "";
            }
        }

        //Generate Gosper nodes based on computed coordinates
        private void GenerateGosperNode(double forwardAngle)
        {
            GosperX += Math.Cos(forwardAngle) * step;
            GosperY -= Math.Sin(forwardAngle) * step;

            IPoint pt = new PointClass();
            pt.X = GosperX;
            pt.Y = GosperY;

            curvePtCol.AddPoint(pt);
        }
    }
}