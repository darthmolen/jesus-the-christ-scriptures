using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

public record TopicalGuide(string Topic, string Language, IReadOnlyList<SubTopic> SubTopics);
