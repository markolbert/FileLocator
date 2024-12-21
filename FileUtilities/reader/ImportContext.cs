using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.FileUtilities;

public class ImportContext( params string[] fieldsToIgnore )
{
    public string ImportPath { get; set; } = null!;
    public bool HasHeaders { get; set; }
    public string? TweaksPath { get; set; }
    public virtual string[] FieldsToIgnore { get; } = fieldsToIgnore;
}