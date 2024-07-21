using System.Linq;
using Tables.Runtime;
using Tables.Sample.Scripts;

namespace Tables.Sample.Editor
{
    public partial class PersonDatabase : IPersonValidator
    {
        public ValidationResult ValidateId(Person self, int id)
        {
            if (Data.Any(v => v != self && v.Id == id))
            {
                return ValidationResult.Error("ID が重複しています");
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateName(Person self, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Error("名前が空です");
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateHeight(Person self, float height)
        {
            if (height < 0)
            {
                return ValidationResult.Error("身長が負です");
            }

            if (height > 300)
            {
                return ValidationResult.Warning("身長が高すぎます");
            }

            return ValidationResult.Success();
        }
    }
}
