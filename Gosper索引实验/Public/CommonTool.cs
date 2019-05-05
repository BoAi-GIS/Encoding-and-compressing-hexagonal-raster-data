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

        public CommonTool(string gdbPath)
        {
            workspaceFactory = new FileGDBWorkspaceFactoryClass();
            pWorkspace = workspaceFactory.OpenFromFile(gdbPath, 0);
            pFeaWorkspace = (IFeatureWorkspace)pWorkspace;
        }

        public IFeatureClass CreateFeatureClass(string feaClassName, esriGeometryType enumGometryType)
        {
            string featureClassName = feaClassName +"Time" +DateTime.Now.ToString("yyyyMMddHHmmss");

            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            ISpatialReferenceFactory spatialrefFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialrefFactory.CreateProjectedCoordinateSystem(
                (int)(esriSRProjCS4Type.esriSRProjCS_Xian1980_GK_CM_111E));//.CreateProjectedCoordinateSystem //esriSRProjCS2Type.esriSRProjCS_WGS1984WorldMercator

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

            #region 文件夹地图要素类属性
            
            IField fieldClass = new FieldClass();
            fieldEdit = (IFieldEdit)fieldClass;
            fieldEdit.Name_2 = "depth";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldname = new FieldClass();
            fieldEdit = (IFieldEdit)fieldname;
            fieldEdit.Name_2 = "name";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldid = new FieldClass();
            fieldEdit = (IFieldEdit)fieldid;
            fieldEdit.Name_2 = "id";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldsize = new FieldClass();
            fieldEdit = (IFieldEdit)fieldsize;
            fieldEdit.Name_2 = "size";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldtype = new FieldClass();
            fieldEdit = (IFieldEdit)fieldtype;
            fieldEdit.Name_2 = "type";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldextend = new FieldClass();
            fieldEdit = (IFieldEdit)fieldextend;
            fieldEdit.Name_2 = "extend";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldcreationTime = new FieldClass();
            fieldEdit = (IFieldEdit)fieldcreationTime;
            fieldEdit.Name_2 = "creationTime";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldlastAccessTime = new FieldClass();
            fieldEdit = (IFieldEdit)fieldlastAccessTime;
            fieldEdit.Name_2 = "lastAccessTime";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldlastWriteTime = new FieldClass();
            fieldEdit = (IFieldEdit)fieldlastWriteTime;
            fieldEdit.Name_2 = "lastWriteTime";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldattributes = new FieldClass();
            fieldEdit = (IFieldEdit)fieldattributes;
            fieldEdit.Name_2 = "attributes";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            fieldsEdit.AddField(fieldUserDefined);
            fieldsEdit.AddField(fieldClass);
            fieldsEdit.AddField(fieldname);
            fieldsEdit.AddField(fieldid);
            fieldsEdit.AddField(fieldsize);

            fieldsEdit.AddField(fieldtype);
            fieldsEdit.AddField(fieldextend);
            fieldsEdit.AddField(fieldcreationTime);
            fieldsEdit.AddField(fieldlastAccessTime);
            fieldsEdit.AddField(fieldlastWriteTime);
            fieldsEdit.AddField(fieldattributes);
            
            #endregion
            #region 自然科学基金地图要素类属性
            /*
            IField fieldid = new FieldClass();
            fieldEdit = (IFieldEdit)fieldid;
            fieldEdit.Name_2 = "id";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fielddepth = new FieldClass();
            fieldEdit = (IFieldEdit)fielddepth;
            fieldEdit.Name_2 = "depth";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;
           
            IField fieldname = new FieldClass();
            fieldEdit = (IFieldEdit)fieldname;
            fieldEdit.Name_2 = "name";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldparentname = new FieldClass();
            fieldEdit = (IFieldEdit)fieldparentname;
            fieldEdit.Name_2 = "parentname";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldslsqxs = new FieldClass();
            fieldEdit = (IFieldEdit)fieldslsqxs;
            fieldEdit.Name_2 = "slsqxs";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeInteger
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldslsqje = new FieldClass();
            fieldEdit = (IFieldEdit)fieldslsqje;
            fieldEdit.Name_2 = "slsqje";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldpzzzxs = new FieldClass();
            fieldEdit = (IFieldEdit)fieldpzzzxs;
            fieldEdit.Name_2 = "pzzzxs";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeInteger
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldpzzzje = new FieldClass();
            fieldEdit = (IFieldEdit)fieldpzzzje;
            fieldEdit.Name_2 = "pzzzje";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldzzjezqwbl = new FieldClass();
            fieldEdit = (IFieldEdit)fieldzzjezqwbl;
            fieldEdit.Name_2 = "zzjezqwbl";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldzzjezxbbl = new FieldClass();
            fieldEdit = (IFieldEdit)fieldzzjezxbbl;
            fieldEdit.Name_2 = "zzjezxbbl";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fielddxpjzzje = new FieldClass();
            fieldEdit = (IFieldEdit)fielddxpjzzje;
            fieldEdit.Name_2 = "dxpjzzje";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldxszzl = new FieldClass();
            fieldEdit = (IFieldEdit)fieldxszzl;
            fieldEdit.Name_2 = "xszzl";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;

            IField fieldjezzl = new FieldClass();
            fieldEdit = (IFieldEdit)fieldjezzl;
            fieldEdit.Name_2 = "jezzl";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;//esriFieldTypeDouble
            fieldEdit.IsNullable_2 = true;
            fieldEdit.Required_2 = true;
          

            fieldsEdit.AddField(fieldUserDefined);
            fieldsEdit.AddField(fieldid);
            fieldsEdit.AddField(fielddepth);

            fieldsEdit.AddField(fieldname);
            fieldsEdit.AddField(fieldparentname);
            fieldsEdit.AddField(fieldslsqxs);
            fieldsEdit.AddField(fieldslsqje);
            fieldsEdit.AddField(fieldpzzzxs);
            fieldsEdit.AddField(fieldpzzzje);
            fieldsEdit.AddField(fieldzzjezqwbl);
            fieldsEdit.AddField(fieldzzjezxbbl);
            fieldsEdit.AddField(fielddxpjzzje);
            fieldsEdit.AddField(fieldxszzl);
            fieldsEdit.AddField(fieldjezzl);
            */
            #endregion 

            UID CLSID = new UIDClass();
            CLSID.Value = "esriGeodatabase.Feature";
            //创建要素类
            IFeatureClass createdFeatureClass = pFeaWorkspace.CreateFeatureClass(featureClassName, fields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", " ");

            return createdFeatureClass;
        }

        public IFeatureClass CreateFeatureClassWithoutAttribute(string feaClassName, esriGeometryType enumGometryType)
        {
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
            //创建要素类
            IFeatureClass createdFeatureClass = pFeaWorkspace.CreateFeatureClass(featureClassName, fields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", " ");

            return createdFeatureClass;
        }

        public IFeatureClass OpenFeatureClass(string name)
        {                      
            //打开要素类
            IFeatureClass tempFeaClass = pFeaWorkspace.OpenFeatureClass(name);           

            return tempFeaClass;
        }

        public IFeatureClass CreateOpenTempFeatureClass(esriGeometryType GeometryType,string name)
        {     
            string tempFeatureclassName = "Temp" + name;
            IFeatureClass tempFeaClass = null;

            if ((pWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, tempFeatureclassName))
            {
                //打开临时要素类
                tempFeaClass = pFeaWorkspace.OpenFeatureClass(tempFeatureclassName);

                IFeatureCursor feaCursor = tempFeaClass.Update(null, false);
                IFeature deleteFea = feaCursor.NextFeature();
                while (deleteFea != null)
                {
                    feaCursor.DeleteFeature();
                    deleteFea = feaCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);
            }
            else
            {
                //在临时数据集中创建临时要素类 
                IFields fields = new FieldsClass();
                IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

                ISpatialReferenceFactory spatialrefFactory = new SpatialReferenceEnvironmentClass();
                ISpatialReference spatialReference = spatialrefFactory.CreateProjectedCoordinateSystem(
                    (int)(esriSRProjCS2Type.esriSRProjCS_WGS1984WorldMercator));//.CreateProjectedCoordinateSystem //esriSRProjCS2Type.esriSRProjCS_WGS1984WorldMercator

                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.GeometryType_2 = GeometryType;
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

                IField fieldClass = new FieldClass();
                fieldEdit = (IFieldEdit)fieldClass;
                fieldEdit.Name_2 = "depth";
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                fieldEdit.IsNullable_2 = true;
                fieldEdit.Required_2 = true;

                IField fieldname = new FieldClass();
                fieldEdit = (IFieldEdit)fieldname;
                fieldEdit.Name_2 = "name";
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.IsNullable_2 = true;
                fieldEdit.Required_2 = true;

                IField fieldsize = new FieldClass();
                fieldEdit = (IFieldEdit)fieldsize;
                fieldEdit.Name_2 = "size";
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                fieldEdit.IsNullable_2 = true;
                fieldEdit.Required_2 = true;

                fieldsEdit.AddField(fieldUserDefined);
                fieldsEdit.AddField(fieldClass);
                fieldsEdit.AddField(fieldname);
                fieldsEdit.AddField(fieldsize);

                UID CLSID = new UIDClass();
                CLSID.Value = "esriGeodatabase.Feature";                
                //创建要素类
                tempFeaClass = pFeaWorkspace.CreateFeatureClass(tempFeatureclassName, fields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", " ");

            }

            return tempFeaClass;
        }
    }
}
