using System.Linq;
using Editor.Data;
using Sample.Scripts;
using UnityEngine;

namespace Sample.Editor
{
    // NOTE: Person から自動生成したい
    [CreateAssetMenu(fileName = nameof(PersonDatabase), menuName = "Databases/" + nameof(PersonDatabase), order = 0)]
    public class PersonDatabase : Database<Person>
    {
        public override object[][] GetData()
        {
            return Data.Select(v => new object[]
                {
                    v.Id,
                    v.Name,
                    v.Height,
                    v.Gender,
                    v.IsMarried,
                }).
                ToArray();
        }
    }
}
