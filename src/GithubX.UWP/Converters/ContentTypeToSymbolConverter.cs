// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Octokit;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace GithubX.UWP.Converters
{
	internal class ContentTypeToSymbolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is ContentType type)
				switch (type)
				{
					case ContentType.Submodule:
						return Symbol.MapDrive;
					case ContentType.Dir:
						return Symbol.Folder;
					default:
						return Symbol.Page;
				}
			else if (value.ToString().Contains("Pull"))
				return Symbol.Shuffle;
			else return Symbol.Import;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
