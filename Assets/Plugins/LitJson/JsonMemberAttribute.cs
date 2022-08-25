using System;

namespace LitJson
{
    public class JsonMemberAttribute : Attribute
    {
        public string Name { get; }
        
        public JsonMemberAttribute(string name)
        {
            Name = name;
        }
    }
}