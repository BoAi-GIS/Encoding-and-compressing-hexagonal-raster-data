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
    /// Summary description for LosslessCompression.
    /// </summary>
    [Guid("a1bf70f5-15bb-42f0-b2e1-f23715056a4b")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.LosslessCompression")]
    public sealed class LosslessCompression : BaseCommand
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

        List<GridUnit> GridUnitList;//Manage element Id,Gosper encoding and Row encoding
        class GridUnit
        {
            public int elementId;//element Id
            public int gosperID;//Gosper encoding
            public int rowID;//Row encoding            
        }


        public LosslessCompression()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text
            base.m_caption = "LosslessCompression";  //localizable text
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
            //1 Getting data that needs lossless compression (Data is stored in the form of feature class)
            IFeatureLayer feaLyr = TargetLayerSelector.OperateLyr as IFeatureLayer;
            IFeatureClass targetFeaClass = feaLyr.FeatureClass;

            //2 Extract the element ID and encoding (Gosper encoding and Row encoding)
            GridUnitList = new List<GridUnit>();
            int elementIdIndex = targetFeaClass.FindField("id");//Get attribute address
            int gosperIdIndex = targetFeaClass.FindField("GosperId");
            int rowIdIndex = targetFeaClass.FindField("RowId");
            List<int> idList = new List<int>();//Record the ID of all elements    

            //Data extraction by traversal
            IFeatureCursor feaCursor = targetFeaClass.Search(null, false);
            IFeature fea = null;
            while ((fea = feaCursor.NextFeature()) != null)
            {
                if (fea.get_Value(elementIdIndex).ToString() == "")
                    continue;
                int elementId = int.Parse(fea.get_Value(elementIdIndex).ToString());//Extract the element ID

                GridUnit gridUnit = new GridUnit();
                gridUnit.gosperID = int.Parse(fea.get_Value(gosperIdIndex).ToString());//Extract the Gosper encoding
                gridUnit.rowID = int.Parse(fea.get_Value(rowIdIndex).ToString());//Extract the Row encoding
                gridUnit.elementId = elementId;
                GridUnitList.Add(gridUnit);

                //Record the element ID   
                if (!idList.Contains(elementId))
                {
                    idList.Add(elementId);
                }
            }

            //3 Information statistics for lossless compression of each element
            //Output address settings for experimental results
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\FolderName";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string txtPath = filePath + "\\FileName" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            StreamWriter sw = new StreamWriter(txtPath, true);

            //Setting of statistical variables
            double gosperLosslessCompressionCountSum = 0;//Gosper lossless compression encoding total amount record
            double gosperStraightforwardCountSum = 0;//Gosper straightforward encoding total amount record
            Dictionary<int, int> gosperAdjSeqLenDic = new Dictionary<int, int>(); // Adjacency sequence length dictionary in Gosper lossless compression

            double rowLosslessCompressionCountSum = 0;//Row lossless compression encoding total amount record
            double rowStraightforwardCountSum = 0;//Row straightforward encoding total amount record
            Dictionary<int, int> rowAdjSeqLenDic = new Dictionary<int, int>(); //Adjacency sequence length dictionary in Row lossless compression

            //3.1 Statistics and output of lossless compression result
            for (int i = 0; i < idList.Count; i++)
            {
                List<int> AdjSeqList = new List<int>();//Adjacency sequence list
                int elementId = idList[i];

                //Get the encoding set of the element region
                List<int> elementGosperIdList = new List<int>();//Gosper encoding set of element region
                List<int> elementRowIdList = new List<int>();//Row encoding set of element region
                GridUnitList.FindAll(a => a.elementId == elementId).ForEach(a =>
                {
                    elementGosperIdList.Add(a.gosperID);
                    elementRowIdList.Add(a.rowID);
                });


                //3.1.1 Statistics of lossless compression result of Gosper encoding
                string gosperStraightforwardStr = "";//Record straightforward encoding
                string gosperRunLengthStr = "";//Record run-length encoding (lossless compression encoding)
                elementGosperIdList.Sort();

                //Straightforward encoding and run-length encoding processing
                int tempId = elementGosperIdList[0];
                gosperStraightforwardStr += tempId.ToString() + ",";
                AdjSeqList.Add(tempId);
                int currentId = tempId;

                for (int v = 1; v < elementGosperIdList.Count; v++)
                {
                    tempId = elementGosperIdList[v];
                    gosperStraightforwardStr += tempId.ToString() + ",";

                    //If the encoding is adjacent, it is added to the adjacency sequence. If not, create a new adjacency sequence to store it
                    if (tempId == currentId + 1)
                    {
                        AdjSeqList.Add(tempId);
                    }
                    else
                    {
                        //Record run-length encoding
                        int unitCodeLen = AdjSeqList.Count;
                        gosperRunLengthStr += AdjSeqList[0] + "," + unitCodeLen.ToString() + ",";

                        //Update the data in adjacency sequence length dictionary of Gosper encoding
                        if (gosperAdjSeqLenDic.ContainsKey(unitCodeLen))
                        {
                            gosperAdjSeqLenDic[unitCodeLen]++;
                        }
                        else
                        {
                            gosperAdjSeqLenDic.Add(unitCodeLen, 1);
                        }

                        AdjSeqList = new List<int>();
                        AdjSeqList.Add(tempId);
                    }
                    currentId = tempId;
                }
                //Processing the last adjacency sequence
                gosperRunLengthStr += AdjSeqList[0] + "," + AdjSeqList.Count.ToString() + ",";
                if (gosperAdjSeqLenDic.ContainsKey(AdjSeqList.Count))
                {
                    gosperAdjSeqLenDic[AdjSeqList.Count]++;
                }
                else
                {
                    gosperAdjSeqLenDic.Add(AdjSeqList.Count, 1);
                }

                //Statistics of Gosper straightforward encoding amount
                double gosperDirectCount = gosperStraightforwardStr.Split(',').Length - 1;
                //Statistics of Gosper run-length encoding amount
                double gosperLosslessCompressionCount = gosperRunLengthStr.Split(',').Length - 1;


                //3.1.2 Statistics of lossless compression result of Row encoding
                string rowStraightforwardStr = "";//Record straightforward encoding
                string rowRunLengthStr = "";//Record run-length encoding (lossless compression encoding)
                elementRowIdList.Sort();

                //Straightforward encoding and run-length encoding processing
                AdjSeqList = new List<int>();
                tempId = elementRowIdList[0];
                rowStraightforwardStr += tempId.ToString() + ",";
                AdjSeqList.Add(tempId);
                currentId = tempId;

                for (int v = 1; v < elementRowIdList.Count; v++)
                {
                    tempId = elementRowIdList[v];
                    rowStraightforwardStr += tempId.ToString() + ",";

                    //If the encoding is adjacent, it is added to the adjacency sequence. If not, create a new adjacency sequence to store it
                    if (tempId == currentId + 1)
                    {
                        AdjSeqList.Add(tempId);

                    }
                    else
                    {
                        //Record run-length encoding
                        int unitCodeLen = AdjSeqList.Count;
                        rowRunLengthStr += AdjSeqList[0] + "," + unitCodeLen.ToString() + ",";

                        //Update the data in adjacency sequence length dictionary of Row encoding
                        if (rowAdjSeqLenDic.ContainsKey(unitCodeLen))
                        {
                            rowAdjSeqLenDic[unitCodeLen]++;
                        }
                        else
                        {
                            rowAdjSeqLenDic.Add(unitCodeLen, 1);
                        }

                        AdjSeqList = new List<int>();
                        AdjSeqList.Add(tempId);
                    }
                    currentId = tempId;
                }

                //Processing the last adjacency sequence
                rowRunLengthStr += "," + AdjSeqList[0] + "," + AdjSeqList.Count.ToString();
                if (rowAdjSeqLenDic.ContainsKey(AdjSeqList.Count))
                {
                    rowAdjSeqLenDic[AdjSeqList.Count]++;
                }
                else
                {
                    rowAdjSeqLenDic.Add(AdjSeqList.Count, 1);
                }

                //Statistics of Row straightforward encoding amount
                double rowDirectCount = rowStraightforwardStr.Split(',').Length - 1;
                //Statistics of Row run-length encoding amount
                double rowLosslessCompressionCount = rowRunLengthStr.Split(',').Length - 1;

                //output of lossless compression result of individual element region
                sw.WriteLine("Element Id," + elementId + ",Gosper straightforward encoding amount," + gosperDirectCount + ",Gosper run-length encoding amount," + gosperLosslessCompressionCount + ",Gosper lossless compression rate," + gosperLosslessCompressionCount / gosperDirectCount + ",Row straightforward encoding amount," + rowDirectCount + ",Row run-length encoding amount," + rowLosslessCompressionCount + ",Row lossless compression rate," + rowLosslessCompressionCount / rowDirectCount);


                //Update overall statistical information
                gosperStraightforwardCountSum += gosperDirectCount;
                gosperLosslessCompressionCountSum += gosperLosslessCompressionCount;

                rowStraightforwardCountSum += rowDirectCount;
                rowLosslessCompressionCountSum += rowLosslessCompressionCount;
            }

            //3.2 Output overall statistical result
            sw.WriteLine("\n");
            sw.WriteLine("Overall statistical results：Total Gosper straightforward encoding amount," + gosperStraightforwardCountSum + ",Total Gosper run-length encoding amount," + gosperLosslessCompressionCountSum + ",Overrall Gosper lossless compression rate," + gosperLosslessCompressionCountSum / gosperStraightforwardCountSum + ",Total Row straightforward encoding amount," + rowStraightforwardCountSum + ",Total Row run-length encoding amount," + rowLosslessCompressionCountSum + ",Overrall Row lossless compression rate," + rowLosslessCompressionCountSum / rowStraightforwardCountSum);
            sw.WriteLine("\n");
            sw.WriteLine("Overrall length statistics of adjacency sequences：");
            sw.WriteLine("Gosper:Length,Amount");

            foreach (KeyValuePair<int, int> kvp in gosperAdjSeqLenDic)
            {
                sw.WriteLine(kvp.Key + "," + kvp.Value);
            }

            sw.WriteLine("Row:Length,Amount");

            foreach (KeyValuePair<int, int> kvp in rowAdjSeqLenDic)
            {
                sw.WriteLine(kvp.Key + "," + kvp.Value);
            }

            sw.Flush();
            sw.Close();
            MessageBox.Show("OK");
        }

        #endregion

    }
}