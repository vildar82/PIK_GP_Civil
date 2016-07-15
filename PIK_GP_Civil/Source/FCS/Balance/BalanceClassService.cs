using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.FCS.Balance
{
    public class BalanceClassService : IClassTypeService
    {
        public const string HomeArea = "Участок дома";
        static List<ClassType> classTypes = new List<ClassType>() {
             new ClassType(HomeArea, "Площадь участка дома", null, 0),
             new ClassType("Размещаемые БРП_ТП", "Площадь размещаемой БРП, ТП", null, 1),
             new ClassType("Жилые здания", "Площадь застройки дома", null, 2),
             new ClassType("Общественные здания", "Площадь застройки дома", null, 2),
             new ClassType("Асфальтобетон", "Площадь проектируемых проездов и автостоянок с покрытием асфальтобетоном", ClassGroup.HardCoating, 3),
             new ClassType("Тротуарная плитка_Тротуары", "Прощадь проектируемых троутаров с покрытием тротуарной плиткой", ClassGroup.HardCoating, 4),
             new ClassType("Тротуарная плитка_Тротуары с проездом", "Площадь проектируемых усиленных тротуаров с покрытием тротуарной плиткой с возможностью проезда пожарного автотранспорта", ClassGroup.HardCoating, 5),
             new ClassType("Газон", "Площадь газонов", ClassGroup.Landscaping, 6),
             new ClassType("Газон_отмостка", "Площадь отмостки с газонным покрытием", ClassGroup.Landscaping, 7),
             new ClassType("Гравийные высевки_ДП", "Детские игровые площадки с покрытием гравийными высевками", ClassGroup.Landscaping, 8),
             new ClassType("Гравийные высевки_ПО", "Площадки отдыха взрослого населения с покрытием гравийными высевками", ClassGroup.Landscaping, 9),
             new ClassType("Гравийные высевки_ПАО", "Площадка активного отдыха гравийными высевками", ClassGroup.Landscaping, 10),
             new ClassType("Газонная решетка_Проезд пож.транспорта", "Площадь проездов пожарного автотранспорта с покрытием газонной решеткой", ClassGroup.Landscaping, 11)
        };

        public ClassType GetClassType (IEnumerable<string> tags)
        {
            foreach (var tag in tags)
            {
                var classType = classTypes.Find(c => c.ClassName.Equals(tag, StringComparison.OrdinalIgnoreCase));
                if (classType != null)
                    return classType;
            }
            return null;
        }
    }
}
