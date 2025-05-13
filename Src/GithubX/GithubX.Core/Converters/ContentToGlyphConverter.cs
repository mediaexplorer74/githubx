using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Platform.Converters;
using Octokit;

namespace GithubXamarin.Core.Converters
{
    public class ContentToGlyphConverter : MvxValueConverter<RepositoryContent, string>
    {
        protected override string Convert(RepositoryContent value, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            switch (value.Type)
            {
                
                case Octokit.ContentType.File:
                    return "\uf1c9";

                case ContentType.Dir:
                    return "\uf07b";

                case ContentType.Symlink:
                    return "";

                case ContentType.Submodule:
                    return "\uf2db";
                

                default:
                    return "";
            }
            */
            if (value.Type == Octokit.ContentType.File)
            {
                return "\uf1c9";
            }
            else if (value.Type == Octokit.ContentType.Dir)
            {
                return "\uf07b";
            }
            else if (value.Type == Octokit.ContentType.Symlink)
            {
                return "";
            }
            else if (value.Type == Octokit.ContentType.Submodule)
            {
                return "\uf2db";
            }
            else
            {
                return "";
            }
        }
    }
}
