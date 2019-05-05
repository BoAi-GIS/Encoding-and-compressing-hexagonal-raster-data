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
    /// Summary description for LossyCompression.
    /// </summary>
    [Guid("243f47ba-5474-49ec-a756-e6918fa023af")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.GosperLossCompressionTool")]
    public sealed class LossyCompression : BaseTool
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
        private IWorkspaceEdit workspaceEdit;

        public LossyCompression()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text 
            base.m_caption = "LossyCompression";  //localizable text 
            base.m_message = "";  //localizable text
            base.m_toolTip = "";  //localizable text
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            //1 Getting data that needs lossy compression (Data is stored in the form of feature class)
            IFeatureLayer feaLyr = TargetLayerSelector.OperateLyr as IFeatureLayer;
            IFeatureClass targetFeaClass = feaLyr.FeatureClass;
            IDataset dataSet = targetFeaClass as IDataset;
            workspaceEdit = dataSet.Workspace as IWorkspaceEdit;

            int elementIdIndex = targetFeaClass.FindField("id");//Get attribute address
            int encodingIndex = targetFeaClass.FindField("GosperId");//If lossy compression of row encoding is carried out, replace "GosperId" here with "RowId"            
            int typeIndex = targetFeaClass.FindField("type");

            //2 Information statistics before fusion
            // Get the element Id set
            List<int> idList = new List<int>();
            IFeatureCursor feaCursor = targetFeaClass.Search(null, false);
            IFeature fea = null;
            while ((fea = feaCursor.NextFeature()) != null)
            {
                string feaIdStr = fea.get_Value(elementIdIndex).ToString();
                if (feaIdStr == "")
                    continue;
                int feaId = int.Parse(feaIdStr);
                //Store the Id set using the list          
                if (!idList.Contains(feaId))
                {
                    idList.Add(feaId);
                }
            }

            //2.1 Number statistics of adjacent sequences before fusion
            List<int> beforeElementAdjSeqNumList = new List<int>();//Number of adjacency sequences in element regions before fusion
            List<int> afterElementAdjSeqNumList = new List<int>();//Number of adjacency sequences in element regions after fusion
            List<int> elementStraightforwardEncAmtList = new List<int>();//Total straightforward encoding amount of element regions
            IQueryFilter qf1 = new QueryFilterClass();//Element querier

            for (int i = 0; i < idList.Count; i++)
            {
                int adjSeqNum = 0;//Adjacency sequences counting variable
                List<int> elementEncodingList = new List<int>();//Storage element encoding set (Gosper encoding or Row encoding based on specific task type)

                // Get the set of element encoding
                qf1.WhereClause = "id = " + idList[i];
                IFeatureCursor feaCursor1 = targetFeaClass.Search(qf1, false);

                IFeature unitFea = null;
                while ((unitFea = feaCursor1.NextFeature()) != null)
                {
                    elementEncodingList.Add(int.Parse(unitFea.get_Value(encodingIndex).ToString()));
                }
                elementEncodingList.Sort();

                // Statistical number of adjacency sequences
                int tempEncoding = elementEncodingList[0];
                int currentEncoding = tempEncoding;

                for (int j = 1; j < elementEncodingList.Count; j++)
                {
                    tempEncoding = elementEncodingList[j];

                    //Encoding breaks, update the number of adjacency sequences
                    if (tempEncoding != currentEncoding + 1)
                    {
                        adjSeqNum++;
                    }

                    currentEncoding = tempEncoding;
                }
                //Add the last adjacency sequence
                adjSeqNum++;

                //Store the adjacency sequences amount and straightforward encoding amount of the region before fusion
                beforeElementAdjSeqNumList.Add(adjSeqNum);
                elementStraightforwardEncAmtList.Add(elementEncodingList.Count);
            }

            //3 Fusion process
            //Output address settings for experimental results
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\FolderName";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string txtPath = filePath + "\\FileName" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            StreamWriter sw = new StreamWriter(txtPath, true);

            //Turn on editing control
            StartEdit();
            int FusionThreshold = 1;//Setting fusion threshold (In this experiment, set 1,2,3 respectively.)

            //3.1 Intermediate absorption fusion
            for (int i = 0; i < idList.Count; i++)
            {
                //Store element encoding set
                List<int> elementEncodingList = new List<int>();

                // Get the set of element encoding
                qf1.WhereClause = "id =" + idList[i];
                IFeatureCursor feaCursor1 = targetFeaClass.Search(qf1, false);

                IFeature unitFea = null;
                while ((unitFea = feaCursor1.NextFeature()) != null)
                {
                    elementEncodingList.Add(int.Parse(unitFea.get_Value(encodingIndex).ToString()));
                }
                elementEncodingList.Sort();

                // Fusion of adjacency sequences according to thresholds
                int tempEncoding = elementEncodingList[0];
                int currentEncoding = tempEncoding;

                IQueryFilter qf = new QueryFilterClass();
                for (int j = 1; j < elementEncodingList.Count; j++)
                {
                    tempEncoding = elementEncodingList[j];

                    //Adjacent encoding
                    if (tempEncoding == currentEncoding + 1)
                    {
                        currentEncoding = tempEncoding;
                        continue;
                    }
                    //Fusion within the threshold range
                    else if (currentEncoding + FusionThreshold >= tempEncoding)
                    {
                        bool isBreak = false;//Indicators for judging whether regional breaks occur

                        //Open editing operation
                        workspaceEdit.StartEditOperation();
                        //Change the belonging attribution of the fused unit
                        for (int k = currentEncoding + 1; k < tempEncoding; k++)
                        {
                            qf.WhereClause = "GosperId =" + k;
                            IFeature searchFea = targetFeaClass.Search(qf, false).NextFeature();
                            string FusionElementId = searchFea.get_Value(elementIdIndex).ToString();

                            searchFea.set_Value(elementIdIndex, idList[i]);//Change the belonging attribution
                            searchFea.set_Value(typeIndex, 1);//Set the change type 1:absorption fusion -1:release fusion
                            searchFea.Store();

                            //Check whether regional break occurs after fusion in the element region corresponding to the unit 
                            if (FusionElementId == "")
                                continue;
                            isBreak = isRegionalBreak(targetFeaClass, int.Parse(FusionElementId.ToString()));

                            if (isBreak)
                            {
                                break;
                            }
                        }
                        //Close editing operation
                        workspaceEdit.StopEditOperation();

                        //As long as there is a break in one element region, the fusion will be cancelled and recover to its original state.
                        if (isBreak)
                        {
                            workspaceEdit.UndoEditOperation();
                        }
                    }
                    currentEncoding = tempEncoding;
                }
            }


            //3.2 Terminal fusion
            for (int i = 0; i < idList.Count; i++)
            {
                //Store element encoding set
                List<int> elementEncodingList = new List<int>();

                // Get the set of element encoding
                qf1.WhereClause = "id =" + idList[i]; //  '" + idList[i] + "'";
                IFeatureCursor feaCursor1 = targetFeaClass.Search(qf1, false);

                IFeature unitFea = null;
                while ((unitFea = feaCursor1.NextFeature()) != null)
                {
                    elementEncodingList.Add(int.Parse(unitFea.get_Value(encodingIndex).ToString()));
                }
                elementEncodingList.Sort();

                //Extraction of short adjacency sequences satisfying threshold constraints
                List<int> shortAdjSeq = new List<int>();
                int tempEncoding = elementEncodingList[0];
                int currentEncoding = tempEncoding;
                shortAdjSeq.Add(tempEncoding);

                IQueryFilter qf = new QueryFilterClass();
                for (int j = 1; j < elementEncodingList.Count; j++)
                {
                    tempEncoding = elementEncodingList[j];

                    //When encoding discontinuity occurs, the adjacency sequence length is judged.
                    if (tempEncoding != currentEncoding + 1)
                    {
                        //Fusion of short adjacency sequences satisfying threshold constraints
                        if (shortAdjSeq.Count <= FusionThreshold)
                        {
                            //Random search for element region adjacent to short adjacent sequence
                            int shortHead = shortAdjSeq[0];
                            int shortTail = shortAdjSeq[shortAdjSeq.Count - 1];

                            qf = new QueryFilterClass();
                            qf.WhereClause = "GosperId =" + (shortHead - 1) + "OR GosperId =" + (shortTail + 1);
                            IFeatureCursor feaCursorNeighbour = targetFeaClass.Search(qf, false);
                            IFeature neighbourElement = feaCursorNeighbour.NextFeature();

                            if (neighbourElement != null)
                            {
                                bool isBreak = false;
                                //Open editing operation
                                workspaceEdit.StartEditOperation();
                                //Modifying the belonging attribution of units corresponding to short adjacency sequences
                                for (int k = 0; k < shortAdjSeq.Count; k++)
                                {
                                    qf.WhereClause = "GosperId =" + shortAdjSeq[k];
                                    IFeature shortFea = targetFeaClass.Search(qf, false).NextFeature();
                                    shortFea.set_Value(elementIdIndex, neighbourElement.get_Value(elementIdIndex));//Change the belonging attribution
                                    shortFea.set_Value(typeIndex, -1);//Set the change type 1:absorption fusion -1:release fusion
                                    shortFea.Store();
                                }

                                //Check whether regional break occurs after fusion in the element region
                                isBreak = isRegionalBreak(targetFeaClass, idList[i]);
                                workspaceEdit.StopEditOperation();

                                //If there is a break in element region, the fusion will be cancelled and recover to its original state.
                                if (isBreak)
                                {
                                    workspaceEdit.UndoEditOperation();
                                }
                            }
                        }
                        shortAdjSeq = new List<int>();
                        shortAdjSeq.Add(tempEncoding);
                        currentEncoding = tempEncoding;
                    }
                    else
                    {
                        shortAdjSeq.Add(tempEncoding);
                        currentEncoding = tempEncoding;
                    }
                }
                //Last adjacency sequence processing
                if (shortAdjSeq.Count <= FusionThreshold)
                {
                    int shortHead = shortAdjSeq[0];
                    int shortTail = shortAdjSeq[shortAdjSeq.Count - 1];

                    qf = new QueryFilterClass();
                    qf.WhereClause = "GosperId =" + (shortHead - 1) + "OR GosperId =" + (shortTail + 1);
                    IFeatureCursor feaCursorNeighbour = targetFeaClass.Search(qf, false);
                    IFeature neighbourElement = feaCursorNeighbour.NextFeature();

                    if (neighbourElement != null)
                    {
                        bool isBreak = false;
                        workspaceEdit.StartEditOperation();
                        for (int k = 0; k < shortAdjSeq.Count; k++)
                        {
                            qf.WhereClause = "GosperId =" + shortAdjSeq[k];
                            IFeature shortFea = targetFeaClass.Search(qf, false).NextFeature();
                            shortFea.set_Value(elementIdIndex, neighbourElement.get_Value(elementIdIndex));
                            shortFea.set_Value(typeIndex, -1);
                            shortFea.Store();
                        }
                        isBreak = isRegionalBreak(targetFeaClass, idList[i]);
                        workspaceEdit.StopEditOperation();

                        if (isBreak)
                        {
                            workspaceEdit.UndoEditOperation();
                        }
                    }
                    shortAdjSeq = null;
                }
            }

            //Turn off editing control, save results
            StopAndCommitEdit();

            //4 Information statistics after fusion
            qf1 = new QueryFilterClass();
            for (int i = 0; i < idList.Count; i++)
            {
                int adjSeqNum = 0;//Adjacency sequences counting variable
                List<int> elementEncodingList = new List<int>();//Storage element encoding set (Gosper encoding or Row encoding based on specific task type)

                // Get the set of element encoding
                qf1.WhereClause = "id =" + idList[i];
                IFeatureCursor feaCursor1 = targetFeaClass.Search(qf1, false);

                IFeature unitFea = null;
                while ((unitFea = feaCursor1.NextFeature()) != null)
                {
                    elementEncodingList.Add(int.Parse(unitFea.get_Value(encodingIndex).ToString()));
                }
                elementEncodingList.Sort();

                // Statistical number of adjacency sequences
                int tempEncoding = elementEncodingList[0];
                int currentEncoding = tempEncoding;

                for (int j = 1; j < elementEncodingList.Count; j++)
                {
                    tempEncoding = elementEncodingList[j];

                    //Encoding breaks, update the number of adjacency sequences
                    if (tempEncoding != currentEncoding + 1)
                    {
                        adjSeqNum++;
                    }

                    currentEncoding = tempEncoding;
                }
                //Add the last adjacency sequence
                adjSeqNum++;
                //Store the adjacency sequences amount of the region after fusion
                afterElementAdjSeqNumList.Add(adjSeqNum);
            }

            //5 Output statistics
            double beforeAdjSeqNumSum = 0;//Total number of adjacency sequences before fusion
            double afterAdjSeqNumSum = 0;//Total number of adjacency sequences after fusion
            double straightforwardEncAmtSum = 0;//Total straightforward encoding amount 
            for (int i = 0; i < idList.Count; i++)
            {
                double beforeAdjSeqNum = beforeElementAdjSeqNumList[i];
                double afterAdjSeqNum = afterElementAdjSeqNumList[i];
                double straightforwardEncAmt = elementStraightforwardEncAmtList[i];

                //Output statistical information of element region 
                sw.WriteLine("Element Id," + idList[i] + ",Number of adjacency sequences before fusion," + beforeAdjSeqNum + ",Number of adjacency sequences after fusion," + afterAdjSeqNum + ",Fusion rate," + (1 - afterAdjSeqNum / beforeAdjSeqNum) + ",compression rate," + (afterAdjSeqNum * 2) / straightforwardEncAmt);

                //Update the overall information
                beforeAdjSeqNumSum += beforeAdjSeqNum;
                afterAdjSeqNumSum += afterAdjSeqNum;
                straightforwardEncAmtSum += straightforwardEncAmt;
            }

            //Output overall statistical information 
            sw.WriteLine(targetFeaClass.AliasName + "综合统计融合前总段数," + beforeAdjSeqNumSum + ",融合后总段数," + afterAdjSeqNumSum + ",总融合率," + (1 - afterAdjSeqNumSum / beforeAdjSeqNumSum) + ",总压缩率," + (afterAdjSeqNumSum * 2) / straightforwardEncAmtSum);
            sw.Flush();
            sw.Close();

            MessageBox.Show("OK");
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add GosperLossCompressionTool.OnMouseDown implementation
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add GosperLossCompressionTool.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add GosperLossCompressionTool.OnMouseUp implementation
        }
        #endregion

        //Function: Judging whether regional breaks have occurred
        public bool isRegionalBreak(IFeatureClass featureClass, int elementId)
        {
            IGeometry geometryBag = new GeometryBagClass();
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = "id = " + elementId;
            IFeatureCursor featureCursor = featureClass.Search(queryFilter, false);

            IGeometryCollection geometryCollection = geometryBag as IGeometryCollection;
            IFeature currentFeature = featureCursor.NextFeature();

            while (currentFeature != null)
            {
                object missing = Type.Missing;
                geometryCollection.AddGeometry(currentFeature.Shape, ref missing, ref missing);
                currentFeature = featureCursor.NextFeature();
            }

            ITopologicalOperator unionedPolygon = new PolygonClass();
            unionedPolygon.ConstructUnion(geometryBag as IEnumGeometry);

            IGeometryCollection geoCol = unionedPolygon as IGeometryCollection;
            if (geoCol.GeometryCount > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Function: Start editing
        private void StartEdit()
        {
            workspaceEdit.StartEditing(true);
        }

        //Function: Stop editing and save
        private void StopAndCommitEdit()
        {
            if (workspaceEdit == null || workspaceEdit.IsBeingEdited() == false)
                return;
            bool hasEdit = false;
            workspaceEdit.HasEdits(ref hasEdit);

            if (hasEdit)
            {

                workspaceEdit.StopEditing(true);
            }
            else
            {
                workspaceEdit.StopEditing(false);
            }
        }
    }
}
