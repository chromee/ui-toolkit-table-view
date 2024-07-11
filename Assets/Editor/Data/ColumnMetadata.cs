using System;
using Editor.VisualElements;

namespace Editor.Data
{
    [Serializable]
    public class ColumnMetadataList
    {
        public ColumnMetadata[] Columns;
    }

    [Serializable]
    public class ColumnMetadata
    {
        public string Name;
        public string Type;
        public float Width;

        public ColInfo ToColInfo()
        {
            var type = Type switch
            {
                "string" => typeof(string),
                "int" => typeof(int),
                "float" => typeof(float),
                "bool" => typeof(bool),
                _ => global::System.Type.GetType(Type),
            };
            return new ColInfo(type, Name, Width);
        }
    }
}
