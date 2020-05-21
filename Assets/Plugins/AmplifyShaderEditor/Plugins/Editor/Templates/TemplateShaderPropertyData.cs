// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TemplateShaderPropertyData
	{
		public string PropertyInspectorName;
		public string PropertyName;
		public WirePortDataType PropertyDataType;
		public PropertyType PropertyType;

		public int Index;
		public string FullValue;
		public string ReplacementValueHelper;

		public TemplateShaderPropertyData(  int index, string fullValue, string propertyInspectorName, string propertyName, WirePortDataType propertyDataType , PropertyType propertyType )
		{
			FullValue = fullValue;
			Index = index;
			PropertyInspectorName = string.IsNullOrEmpty( propertyInspectorName )?propertyName: propertyInspectorName;
			PropertyName = propertyName;
			PropertyDataType = propertyDataType;
			PropertyType = propertyType;
			int idx = FullValue.LastIndexOf( "=" );
			ReplacementValueHelper = ( idx >= 0 ) ? FullValue.Substring( 0, idx + 1 ) +" ": FullValue + " = ";
		}

		public string CreatePropertyForValue( string value )
		{
			return ReplacementValueHelper + value;
		}

		public override string ToString()
		{
			return string.Format( "{0}(\"{1}\", {2})", PropertyName, PropertyInspectorName,UIUtils.WirePortToCgType( PropertyDataType ) );
		}
	}
}
