using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;

namespace Compression
{
    /// <summary>
    /// Summary description for TargetLayerSelector.
    /// </summary>
    [Guid("E28AE0EE-9C6B-4D91-BC96-7B71B2887AFE")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Compression.TargetLayerSelector")]
    public sealed class TargetLayerSelector : BaseCommand, IToolControl
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

        private IApplication mApplication;
        private ComboBox lyrSelectorCb;
        private Dictionary<string, ILayer> dicLayers;
        IMap pMap = null;

        private static ILayer operateLyr;
        public static ILayer OperateLyr
        {
            get
            {
                return operateLyr;
            }
        }

        public TargetLayerSelector()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text
            base.m_caption = "";  //localizable text
            base.m_message = "";  //localizable text 
            base.m_toolTip = "";  //localizable text 
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

            lyrSelectorCb = new ComboBox();
            lyrSelectorCb.DropDownStyle = ComboBoxStyle.DropDownList;
            lyrSelectorCb.Size = new System.Drawing.Size(100, 24);
            lyrSelectorCb.Click += new EventHandler(lyrSelectorCb_Click);
            lyrSelectorCb.SelectedIndexChanged += new EventHandler(lyrSelectorCb_SelectedIndexChanged);

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

        void lyrSelectorCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            operateLyr = dicLayers[lyrSelectorCb.SelectedItem.ToString()] as IFeatureLayer;
        }

        void lyrSelectorCb_Click(object sender, EventArgs e)
        {
            this.OnClick();
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

            mApplication = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
            pMap = (mApplication.Document as IMxDocument).ActiveView.FocusMap;
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add OperateLayerSelector.OnClick implementation
            if (mApplication != null)
            {
                IEnumLayer enumLayer = pMap.get_Layers();
                ILayer layer = null;
                enumLayer.Reset();
                dicLayers = new Dictionary<string, ILayer>();
                this.lyrSelectorCb.Items.Clear();
                while ((layer = enumLayer.Next()) != null)
                {
                    if (!layer.Visible)
                        continue;
                    if (!dicLayers.ContainsKey(layer.Name))
                    {
                        dicLayers.Add(layer.Name, layer);
                        lyrSelectorCb.Items.Add(layer.Name);
                    }
                }
            }
        }

        #endregion

        #region IToolControl 成员

        public bool OnDrop(esriCmdBarType barType)
        {
            return true;
            //throw new NotImplementedException();
        }

        public void OnFocus(ICompletionNotify complete)
        {
            //throw new NotImplementedException();
        }

        public int hWnd
        {
            get { return lyrSelectorCb.Handle.ToInt32(); }
        }

        #endregion
    }
}
