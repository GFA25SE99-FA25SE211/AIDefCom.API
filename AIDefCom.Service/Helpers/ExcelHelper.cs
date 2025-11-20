using OfficeOpenXml;

namespace AIDefCom.Service.Helpers
{
    public static class ExcelHelper
    {
        private static bool _isConfigured = false;
        private static readonly object _lock = new object();

        public static void ConfigureExcelPackage()
        {
            if (!_isConfigured)
            {
                lock (_lock)
                {
                    if (!_isConfigured)
                    {
                        // For EPPlus 7.x: This works perfectly
                        // For non-commercial use only. For commercial use, purchase a license from EPPlus Software
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        _isConfigured = true;
                    }
                }
            }
        }
    }
}
