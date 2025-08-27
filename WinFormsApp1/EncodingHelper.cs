using System.Text;

namespace WinFormsApp1
{
    public static class EncodingHelper
    {
        static EncodingHelper()
        {
            // Регистрируем провайдер для доступа к дополнительным кодировкам
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static List<EncodingItem> GetAllEncodings()
        {
            var encodingItems = new List<EncodingItem>();

            var encodingInfos = Encoding.GetEncodings();

            foreach (var encodingInfo in encodingInfos)
            {
                var encoding = encodingInfo.GetEncoding();

                encodingItems.Add(new EncodingItem
                {
                    CodePage = encoding.CodePage,
                    Name = $"{encodingInfo.Name} ({encoding.EncodingName})"
                });
            }

            return encodingItems;
        }

        public static List<EncodingItem> GetEncodingsByCategory(int categoryId)
        {
            var allEncodings = GetAllEncodings();
            var result = new List<EncodingItem>();

            foreach (var encoding in allEncodings)
            {
                // Создаем временный EncodingInfo для определения категории
                var encodingInfo = new EncodingInfo(encoding.CodePage, encoding.Name);
                if (GlobalEncodings.GetEncodingCategory(encodingInfo) == categoryId)
                {
                    result.Add(encoding);
                }
            }

            return result.OrderBy(e => e.CodePage).ToList();
        }

        public static EncodingItem GetEncodingByCodePage(int codePage)
        {
            var allEncodings = GetAllEncodings();
            return allEncodings.FirstOrDefault(e => e.CodePage == codePage);
        }
    }

    public class EncodingInfo
    {
        public int CodePage { get; }
        public string Name { get; }

        public EncodingInfo(int codePage, string name)
        {
            CodePage = codePage;
            Name = name;
        }
    }
}