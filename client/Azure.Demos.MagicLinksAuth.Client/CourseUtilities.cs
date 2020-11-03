using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.Demos.MagicLinksAuth.Client
{
    public class CourseUtilities
    {
        internal static string FormatChapterName(string chapterSlug)
        {
            var chapterName = chapterSlug.Substring(2).Replace('-', ' ');
            return char.ToUpper(chapterName[0]) + chapterName.Substring(1);
        }
    }
}
