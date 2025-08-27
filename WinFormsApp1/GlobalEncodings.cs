using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public static class GlobalEncodings
    {
        // Регистрируем провайдер кодировок
        static GlobalEncodings()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static List<EncodingInfo> GetAllEncodings()
        {
            var allEncodings = new List<EncodingInfo>();

            try
            {
                // Получаем все доступные кодировки
                allEncodings.AddRange(Encoding.GetEncodings().Select(x =>
                {
                    return new EncodingInfo(x.CodePage, x.Name);
                }));

                // Дополнительные кодировки через CodePages
                var codePages = Encoding.GetEncodings()
                    .Where(e => e.CodePage > 1000) // Фильтруем стандартные
                    .ToList();

                allEncodings.AddRange(codePages.Select(x =>
                {
                    return new EncodingInfo(x.CodePage, x.Name);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения кодировок: {ex.Message}");
            }

            return allEncodings.DistinctBy(e => e.CodePage).ToList();
        }

        public static Dictionary<int, string> GetEncodingCategories()
        {
            return new Dictionary<int, string>
        {
            { 0, "Unicode и UTF" },
            { 1, "Windows Code Pages" },
            { 2, "ISO Standards" },
            { 3, "IBM/OEM Code Pages" },
            { 4, "National Standards" },
            { 5, "Macintosh" },
            { 6, "EBCDIC" },
            { 7, "Другие и специальные" }
        };
        }

        public static int GetEncodingCategory(EncodingInfo encoding)
        {
            int codePage = encoding.CodePage;

            // Unicode
            if (codePage == 65001 || codePage == 65000 || codePage == 1200 ||
                codePage == 1201 || codePage == 12000 || codePage == 12001)
                return 0;

            // Windows
            if (codePage >= 1250 && codePage <= 1258)
                return 1;

            // ISO
            if ((codePage >= 28591 && codePage <= 28605) ||
                (codePage >= 8859 && codePage <= 8860))
                return 2;

            // IBM/OEM
            if ((codePage >= 437 && codePage <= 950) ||
                codePage == 852 || codePage == 855 || codePage == 857 ||
                codePage == 860 || codePage == 861 || codePage == 862 ||
                codePage == 863 || codePage == 864 || codePage == 865 ||
                codePage == 866 || codePage == 869)
                return 3;

            // National
            if (codePage == 20866 || codePage == 21866 || codePage == 1251 ||
                codePage == 866 || codePage == 10007 || codePage == 10029)
                return 4;

            // Macintosh
            if (codePage >= 10000 && codePage <= 10079)
                return 5;

            // EBCDIC
            if (codePage >= 37 && codePage <= 1149 && codePage % 10 == 0)
                return 6;

            return 7; // Другие
        }
    }
}
