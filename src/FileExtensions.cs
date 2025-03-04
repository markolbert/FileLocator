#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileExtensions.cs
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

using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static class FileExtensions
{
    public static bool ValidateFilePath(
        string path,
        out string? result,
        string? reqdExtension = ".json",
        IEnumerable<string>? folders = null,
        bool requireWriteAccess = false,
        ILogger? logger = null
    )
    {
        logger?.PathValidationStart( path );

        folders ??= [];

        var extension = Path.GetExtension( path );
        if( reqdExtension != null && !extension.Equals( reqdExtension, StringComparison.OrdinalIgnoreCase ) )
        {
            path = Path.GetFileNameWithoutExtension( path ) + reqdExtension;
            logger?.AddedRequiredExtension( reqdExtension );
        }

        var fileOkay = requireWriteAccess
            ? CheckFileCanBeCreated( path, out result, logger )
            : CheckFileExists( path, out result, logger );

        if( fileOkay )
        {
            logger?.FoundFile( result! );
            return true;
        }

        // we didn't find the file based just on its current path, so
        // look for it in the folders we were given. How we do this
        // depends on whether the path we were given is rooted.
        var folderList = folders.ToList();

        var directoryPath = Path.GetDirectoryName( path );
        if( !Path.IsPathRooted( directoryPath ) )
        {
            if( CheckAlternativeLocations( path,
                                           folderList,
                                           requireWriteAccess,
                                           out result,
                                           logger ) )
            {
                logger?.FoundFile( result! );
                return true;
            }
        }

        path = Path.GetFileName( path );

        if( CheckAlternativeLocations( Path.GetFileName( path ), folderList, requireWriteAccess, out result, logger ) )
        {
            logger?.FoundFile( result! );
            return true;
        }

        logger?.FileNotFound( path );

        return false;
    }

    private static bool CheckAlternativeLocations(
        string path,
        List<string> folderList,
        bool requireWriteAccess,
        out string? result,
        ILogger? logger
    )
    {
        result = null;
        string? pathToCheck = null;

        foreach( var folder in folderList )
        {
            logger?.CheckAlternativeLocation( folder, path );

            try
            {
                pathToCheck = Path.Combine( folder, path );

                if( requireWriteAccess
                       ? CheckFileCanBeCreated( pathToCheck, out result, logger )
                       : CheckFileExists( pathToCheck, out result, logger ) )
                    return true;
            }
            catch( Exception ex )
            {
                logger?.Exception( $"checking path {pathToCheck!}", ex.Message );
                return false;
            }
        }

        return false;
    }

    private static bool CheckFileExists( string path, out string? result, ILogger? logger )
    {
        result = null;

        bool fileExists;

        try
        {
            fileExists = File.Exists( path );
        }
        catch
        {
            logger?.FileNotAccessible( path );
            return false;
        }

        if( fileExists )
        {
            result = path;
            return true;
        }

        logger?.FileNotFound( path );
        return false;
    }

    private static bool CheckFileCanBeCreated( string path, out string? result, ILogger? logger )
    {
        //see if the file can be created where specified
        var directory = Path.GetDirectoryName( path )!;
        var testFile = Path.Combine( directory, Guid.NewGuid().ToString() );

        var canCreate = false;

        try
        {
            using var fs = File.Create( testFile );

            fs.Close();
            File.Delete( testFile );

            canCreate = true;
        }
        catch( Exception ex )
        {
            logger?.Exception( $"access directory {directory}", ex.Message );
        }

        result = canCreate ? path : null;

        return !string.IsNullOrEmpty( result );
    }
}
