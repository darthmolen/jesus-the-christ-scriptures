using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

// Key is the language-invariant slug (set by the loader from short_en ?? short).
public record SubTopic(string Key, string Title, string Short, IReadOnlyList<Reference> References);
