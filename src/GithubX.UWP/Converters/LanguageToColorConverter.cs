// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Windows.UI.Xaml.Data;

namespace GithubX.UWP.Converters
{
	internal class LanguageToColorConverter:IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
			=> Shared.Helpers.OctokitHelper.FindLanguageColor((string)value);

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
