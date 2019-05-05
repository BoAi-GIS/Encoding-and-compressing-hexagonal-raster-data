using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace Compression
{
    class CommonTool
    {
        IWorkspaceFactory workspaceFactory;    
        public IWorkspace pWorkspace;
        public IFeatureWorkspace pFeaWorkspace;

        /// <summary>
        /// Common tools for manipulating GDB data
        /// </summary>
        /// <param name="gdbPath">the GDB file path</param>
        public CommonTool(string gdbPath)
        {
            workspaceFactory = new FileGDBWorkspaceFactoryClass();
            pWorkspace = workspaceFactory.OpenFromFile(gdbPath, 0);
            pFeaWorkspace = (IFeatureWorkspace)pWorkspace;
        }

        public IFeatureClass CreateFeatureClassWithoutAttribute(string feaClassName, esriGeometryType enumGometryType)
        {
            //Basic settings of FeatureClass
            string featureClassName = feaClassName + "Time" + DateTime.Now.ToString("yyyyMMddHHmmss");

            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            ISpatialReferenceFactory spatialrefFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialrefFactory.CreateProjectedCoordinateSystem(
                (int)(esriSRProjCS4Type.esriSRProjCS_Xian1980_GK_CM_111E));
            IGeometryDef geometryDef = new GeometryDefClass();

            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = enumGometryType;
            geometryDefEdit.GridCount_2 = 1;
            geometryDefEdit.set_GridSize(0, 0);
            geometryDefEdit.SpatialReference_2 = spatialReference;

            IField fieldUserDefined = new FieldClass();
            IFieldEdit fieldEdit = (IFieldEdit)fieldUserDefined;
            fieldEdit.Name_2 = "Shape";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            fieldsEdit.AddField(fieldUserDefined);

            UID CLSID = new UIDClass();
            CLSID.Value = "esriGeodatabase.Feature";
            

            //Create FeatureClass
            IFeatureClass createdFeatureClass = pFeaWorkspace.CreateFeatureClass(featureClassName, fields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", " ");

            return createdFeatureClass;
        }

        public IFeatureClass OpenFeatureClass(string name)
        {
            //Open FeatureClass
            IFeatureClass tempFeaClass = pFeaWorkspace.OpenFeatureClass(name);           

            return tempFeaClass;
        }

    }
}
