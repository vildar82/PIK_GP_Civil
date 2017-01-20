using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.Surface.UI
{
    public class SurfaceLabelViewModel
    {
        public SurfaceLabelViewModel(List<string> slopeLabelStyles, List<string> spotElevationLabelStyles, string surface)
        {
            SlopeLabelStyles = slopeLabelStyles;
            SpotElevationLabelStyles = spotElevationLabelStyles;
        }

        public List<string> SlopeLabelStyles { get; set; }
        public List<string> SpotElevationLabelStyles { get; set; }
    }
}
