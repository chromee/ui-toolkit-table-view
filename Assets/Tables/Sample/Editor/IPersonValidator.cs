using System.Linq;
using Tables.Runtime;
using Tables.Sample.Scripts;

namespace Tables.Sample.Editor
{
    // 自動生成予定
    public partial class PersonDatabase : Database<Person>
    {
        public override object[][] GetDataAsArray()
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

        public ValidationResult Validate(ColumnMetadata column, Person person, object value)
        {
            if (person == null) return ValidationResult.Success();

            var self = (IPersonValidator)this;
            switch (column.Name)
            {
                case nameof(Person.Id):
                    return self.ValidateId(person, (int)value);
                case nameof(Person.Name):
                    return self.ValidateName(person, (string)value);
                case nameof(Person.Height):
                    return self.ValidateHeight(person, (float)value);
                case nameof(Person.Gender):
                    return self.ValidateGender(person, (Gender)value);
                case nameof(Person.IsMarried):
                    return self.ValidateIsMarried(person, (bool)value);
                default:
                    return ValidationResult.Success();
            }
        }
    }

    public interface IPersonValidator : IValidator<Person>
    {
        ValidationResult ValidateId(Person self, int id) => ValidationResult.Success();
        ValidationResult ValidateName(Person self, string name) => ValidationResult.Success();
        ValidationResult ValidateHeight(Person self, float height) => ValidationResult.Success();
        ValidationResult ValidateGender(Person self, Gender gender) => ValidationResult.Success();
        ValidationResult ValidateIsMarried(Person self, bool isMarried) => ValidationResult.Success();
    }
}
