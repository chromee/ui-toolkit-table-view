using System.IO;
using Editor.Data;
using UnityEngine;

namespace Editor.Sample
{
    public class PersonDatabase
    {
        public ColumnMetadata[] Columns;
        public Person[] Persons;

        public PersonDatabase()
        {
            var columnJson = File.ReadAllText(Application.dataPath + "/Sample/person_matadata.json");
            Columns = JsonUtility.FromJson<ColumnMetadataList>(columnJson)?.Columns;

            var dataJson = File.ReadAllText(Application.dataPath + "/Sample/person.json");
            Persons = JsonUtility.FromJson<PersonList>(dataJson)?.Persons;
        }
    }
}
