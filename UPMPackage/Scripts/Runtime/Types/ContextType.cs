namespace RTLTMPro {
  public enum ContextType {
    // "Tag" represents that the associated character is in a valid tag range
    Tag = -1,
    // "Default" represents that the context type of associated character has not been assigned yet
    Default = 0,
    LeftToRight = 1,
    RightToLeft = 2,
  }
}
