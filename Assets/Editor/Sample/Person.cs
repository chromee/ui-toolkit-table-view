using System;

namespace Editor.Sample
{
    [Serializable]
    public class PersonList
    {
        public Person[] Persons;
    }
    
    [Serializable]
    public class Person
    {
        public int Id;
        public string Name;
        public float Height;
        public Gender Gender;
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
    }
}
