using System.Globalization;
using System.Windows.Data;

namespace WeComLoad.Open.Common.Converters
{
    public class StatusToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // customized_app_status  0：待开发；1：开发中；2：已上线
            // auditorder.status 0：*；1：*；2：待上线；3：*；4：已取消上线；5：已上线
            var content = "开发并上线";
            if (value is CorpApp corpApp && value != null)
            {
                if (corpApp.customized_app_status == 1)
                {
                    content = "审核并上线";

                    if (corpApp.auditorder != null && (corpApp.auditorder.status == 2 || corpApp.auditorder.status == 4))
                    {
                        content = "提交上线";
                    }
                }
            }
            return content;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
