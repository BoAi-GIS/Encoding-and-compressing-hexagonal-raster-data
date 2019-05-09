using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace Compression
{
    /// <summary>
    /// Summary description for GosperIndexToolbar.
    /// </summary>
    [Guid("0100c811-df33-4a19-9245-a99c8a9109bc")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.GosperCodeToolbar")]
    public sealed class GosperCodeToolbar: BaseToolbar
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
            MxCommandBars.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommandBars.Unregister(regKey);
        }

        #endregion
        #endregion

        public GosperCodeToolbar()
        {
            AddItem(typeof(TargetLayerSelector));//Add target layer select tool
            AddItem(typeof(GenerateGosperCurve));//Add Gosper curve generation tool
            AddItem(typeof(LosslessCompression));//Add lossless compression tool
            AddItem(typeof(LossyCompression));//Add lossy compression tool
            AddItem(typeof(AccuracyLossStatistics));//Add lossy compression accuracy loss statistical tool        
        }

        public override string Caption
        {
            get
            {
                //TODO: Replace bar caption
                return "A_GosperCode Toolbar";
            }
        }
        public override string Name
        {
            get
            {
                //TODO: Replace bar ID
                return "GosperCodeToolbar";
            }
        }
    }
}