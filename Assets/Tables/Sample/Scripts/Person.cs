using System;

namespace Tables.Sample.Scripts
{
    [Serializable]
    public class Person
    {
        public int Id;
        public string Name;
        public float Height;
        public Gender Gender;
        public bool IsMarried;
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
    }
}
