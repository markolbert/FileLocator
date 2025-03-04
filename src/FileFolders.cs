﻿#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileFolders.cs
//
// This file is part of JumpForJoy Software's DependencyInjection.
// 
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections;

namespace J4JSoftware.FileUtilities;

public class FileFolders : IEnumerable<string>
{
    public static FileFolders Default { get; } = [];

    private readonly List<string> _folders = [];

    public FileFolders(
        bool inclCurDir = true,
        bool inclEntryDir = true
    )
    {
        if( inclCurDir )
            _folders.Add( Environment.CurrentDirectory );

        if( !inclEntryDir )
            return;

        _folders.Add( AppDomain.CurrentDomain.BaseDirectory );
    }

    public void Add( string folder )
    {
        _folders.Add( folder );
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach( var folder in _folders )
        {
            yield return folder;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
