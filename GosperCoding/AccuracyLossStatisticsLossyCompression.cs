using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace Compression
{
    /// <summary>
    /// Summary description for GosperLossCompressionResultStatistics.
    /// </summary>
    [Guid("65733d30-0930-4edd-b2cd-58720d1664b8")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.AccuracyLossStatistics")]
    public sealed class AccuracyLossStatistics : BaseCommand
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
        public AccuracyLossStatistics()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text
            base.m_caption = "AccuracyLossStatistics";  //localizable text
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
            //1 Get two data before and after lossy compression (Data is stored in the form of feature class)
            string gdbPath = @"filePath\fileName.gdb";
            string beforeFeaClassName = "FeatureClassNameXXX";
            string afterFeaClassName = "FeatureClassNameXXX";

            CommonTool CommonTool = new CommonTool(gdbPath);
            IFeatureClass beforeFeaClass = CommonTool.OpenFeatureClass(beforeFeaClassName);//Data before lossy compression
            IFeatureClass afterFeaClass = CommonTool.OpenFeatureClass(afterFeaClassName);//data after lossy compression
            int elementIdIndex = beforeFeaClass.FindField("id");//Get attribute address

            //2 Statistics and output of accuracy loss results
            // Output address settings for experimental results
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\FolderName";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string txtPath = filePath + "\\FileName" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            StreamWriter sw = new StreamWriter(txtPath, true);

            // Get the element Id set
            List<int> idList = new List<int>();
            IFeatureCursor feaCursor = beforeFeaClass.Search(null, false);
            IFeature fea = null;
            while ((fea = feaCursor.NextFeature()) != null)
            {
                string feaIdStr = fea.get_Value(elementIdIndex).ToString();
                if (feaIdStr == "")
                    continue;
                int feaId = int.Parse(feaIdStr);
                if (!idList.Contains(feaId))
                {
                    idList.Add(feaId);
                }
            }

            double geometricChangeRatioSum = 0;//Total loss of geometric accuracy
            double semanticChangeRatioSum = 0;//Total loss of semantic accuracy
            IQueryFilter qf = new QueryFilterClass();
            for (int i = 0; i < idList.Count; i++)
            {
                qf.WhereClause = "id =" + idList[i];

                // Statistical differences in the number of units before and after fusion
                double beforeUnitCount = beforeFeaClass.FeatureCount(qf);
                double afterUnitCount = afterFeaClass.FeatureCount(qf);
                double geometricChangeRatio = Math.Abs(beforeUnitCount - afterUnitCount) / beforeUnitCount;
                geometricChangeRatioSum += geometricChangeRatio;

                //Statistical semantic accuracy loss
                //Get the ObjectID of units in element region before fusion
                IFeatureCursor fCursor = beforeFeaClass.Search(qf, false);
                IFeature f = null;
                List<int> beforeIdList = new List<int>();

                while ((f = fCursor.NextFeature()) != null)
                {
                    beforeIdList.Add(f.OID);
                }
                double beforCount = beforeIdList.Count;//the number of units in element region before fusion

                //Get the ObjectID of units in element region after fusion
                fCursor = afterFeaClass.Search(qf, false);
                f = null;
                List<int> afterIdList = new List<int>();

                while ((f = fCursor.NextFeature()) != null)
                {
                    afterIdList.Add(f.OID);
                }

                double afterCount = afterIdList.Count;//the number of units in element region after fusion
                double coincidenceCount = 0;//the number of coincidence units

                //Get the number of coincidence units in two regions before and after fusion
                for (int j = 0; j < beforeIdList.Count; j++)
                {
                    if (afterIdList.Contains(beforeIdList[j]))
                    {
                        coincidenceCount++;
                    }
                }

                double semanticChangeCountRatio = (Math.Abs(beforCount - coincidenceCount) + Math.Abs(afterCount - coincidenceCount)) / beforCount;//Get the semantic accuracy loss
                semanticChangeRatioSum += semanticChangeCountRatio;//Update the semantic accuracy loss

                //output result of individual element region
                sw.WriteLine("Element Id," + idList[i] + ",Units number before fusion," + beforeUnitCount + ",Units number after fusion," + afterUnitCount + ",Geometric accuracy loss," + geometricChangeRatio + ",Semantic accuracy loss," + semanticChangeCountRatio);
                sw.Flush();
            }

            // Output overall statistical result
            sw.WriteLine("Average geometric accuracy loss," + geometricChangeRatioSum / idList.Count + ",Average semantic accuracy loss," + semanticChangeRatioSum / idList.Count);
            sw.Flush();
            sw.Close(); ;
            MessageBox.Show("OK");
        }

        #endregion
    }
}
