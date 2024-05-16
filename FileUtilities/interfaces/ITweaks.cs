﻿using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.FileUtilities;

public interface ITweaks
{
    string KeyFieldName { get; }
    ReadOnlyDictionary<int, Tweak> Collection { get; }
    bool AllComplete { get; }

    IFileContext? Source { get; set; }

    bool Load();

    void ApplyTweaks( object entity );
}

public interface ITweaks<in TEntity> : ITweaks
    where TEntity : class
{
    void ApplyTweaks( TEntity entity );
}
