using System.Globalization;
using System.Windows.Data;

namespace WeComLoad.Open.Common.Converters
{
    public class StatusToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = "审核上线";
            if (value is CorpApp corpApp && value != null)
            {
                if (corpApp.customized_app_status == 1)
                {
                    content = "提交审核";
                }
                else if (corpApp.customized_app_status == 2)
                {
                    if (corpApp.auditorder.status == 2 || corpApp.auditorder.status == 4)
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
