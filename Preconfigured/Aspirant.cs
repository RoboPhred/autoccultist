using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Preconfigured
{
    public class Aspirant
    {
        public static BrainConfig Config
        {
            get
            {
                return new BrainConfig
                {
                    Goals = new List<Goal> {
                        new Goal {
                            Name = "Begin the Intro",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "introjob"}
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "bequestintro"}
                                },
                            },
                            Imperatives = new List<Imperative> {
                                new Imperative {
                                    Name = "Work the intro job",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "work",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementID = "introjob"}}
                                        }
                                    },
                                }
                            }
                        },
                        new Goal {
                            Name = "Study Bequest",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "bequestintro"}
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "ascensionenlightenmenta"}
                                }
                            },
                            Imperatives = new List<Imperative> {
                                HardLabor,
                                new Imperative {
                                    Name = "Study the Bequest",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementID = "bequestintro"}},
                                            {"Approach", new CardChoice { ElementID = "reason"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Find a Collaborator",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "contactintro"}
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice {
                                        Aspects = new Dictionary<string, int> {
                                            {"acquaintance", 1}
                                        }
                                    }
                                }
                            },
                            Imperatives = new List<Imperative> {
                                HardLabor,
                                PaintAwayRestlessness,
                                new Imperative {
                                    Name = "Contact Colaborator",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementID = "contactintro"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Get Swole 1",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "health"}
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Mode = CardsSatisfiedMode.Any,
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "skillhealtha"},
                                    new CardChoice { ElementID = "skillhealthb"},
                                    new CardChoice { ElementID = "skillhealthc"}
                                }
                            },
                            Imperatives = new List<Imperative> {
                                HardLabor,
                                PaintAwayRestlessness,
                                new Imperative {
                                    Name = "Increase health skill from Vitality",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice{ElementID = "vitality"}},
                                            {"morevitality", new CardChoice {ElementID = "vitality"}}
                                        }
                                    }
                                },
                                RefreshHealth
                            }
                        },
                        new Goal {
                            Name = "Get Swole 2",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "skillhealtha"}
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Mode = CardsSatisfiedMode.Any,
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "skillhealthb"},
                                    new CardChoice { ElementID = "skillhealthc"}
                                }
                            },
                            Imperatives = new List<Imperative> {
                                RefreshHealth,
                                new Imperative {
                                    Name = "Excersize",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            { "study", new CardChoice{ElementID = "health"}}
                                        }
                                    }
                                },
                                PaintAwayRestlessness,
                                HealtAfflictionWithFunds,
                                new Imperative {
                                    Name = "Learn a lesson from Vitality",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice{ElementID = "vitality"}},
                                            {"morevitality", new CardChoice {ElementID = "vitality"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Stronger Physique work",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "work",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our skill card if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementID = "skillhealtha"}},
                                            {"Health", new CardChoice { ElementID = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Get Swole 2",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementID = "skillhealtha"}},
                                            {"V1", new CardChoice {ElementID="vitalityplus"}},
                                            {"V2", new CardChoice {ElementID="vitalityplus"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Get Swole 3",
                            RequiredCards = new CardsSatisfiedCondition {
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "skillhealthb" }
                                }
                            },
                            CompletedByCards = new CardsSatisfiedCondition {
                                Mode = CardsSatisfiedMode.Any,
                                Cards = new List<CardChoice> {
                                    new CardChoice { ElementID = "skillhealthc" }
                                }
                            },
                            Imperatives = new List<Imperative> {
                                RefreshHealth,
                                PaintAwayRestlessness,
                                HealtAfflictionWithFunds,
                                new Imperative {
                                    Name = "Excersize",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            { "study", new CardChoice{ElementID = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Learn a lesson from Vitality",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice{ElementID = "vitality"}},
                                            {"morevitality", new CardChoice {ElementID = "vitality"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Hardened Physique work",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "work",
                                    ForbidWhenCardsPresent = new CardsSatisfiedCondition {
                                        Cards = new List<CardChoice> {
                                            // Do not monopolize our skill card if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                            new CardChoice { ElementID = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementID = "skillhealthb"}},
                                            {"Health", new CardChoice { ElementID = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Get Swole 3",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementID = "skillhealthb"}},
                                            {"V1", new CardChoice {ElementID="vitalityplus"}},
                                            {"V2", new CardChoice {ElementID="vitalityplus"}},
                                            {"V3", new CardChoice {ElementID="vitalityplus"}}
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }

        private static Imperative HardLabor = new Imperative
        {
            Name = "Work for a living",
            Priority = ImperativePriority.Maintenance,
            Verb = "work",
            StartingRecipe = new RecipeSolution
            {
                Slots = new Dictionary<string, CardChoice> {
                    {"work", new CardChoice { ElementID = "health"}}
                }
            }
        };
        private static Imperative RefreshHealth = new Imperative
        {
            Name = "Refresh health",
            Priority = ImperativePriority.Maintenance,
            Verb = "dream",
            StartingRecipe = new RecipeSolution
            {
                Slots = new Dictionary<string, CardChoice> {
                    {"dream", new CardChoice{ElementID = "fatigue"}}
                }
            }
        };

        private static Imperative PaintAwayRestlessness = new Imperative
        {
            Name = "Paint away Restlessness",
            Priority = ImperativePriority.Critical,
            Verb = "work",
            StartingRecipe = new RecipeSolution
            {
                Slots = new Dictionary<string, CardChoice> {
                    {"work", new CardChoice {ElementID = "passion"}}
                }
            },
            OngoingRecipes = new Dictionary<string, RecipeSolution> {
                {"paintbasic", new RecipeSolution {
                    Slots = new Dictionary<string, CardChoice> {
                        {"Yearning", new CardChoice {ElementID = "restlessness"}}
                    }
                }}
            }
        };

        private static Imperative HealtAfflictionWithFunds = new Imperative
        {
            Name = "Heal Affliction with Money",
            Priority = ImperativePriority.Critical,
            Verb = "dream",
            StartingRecipe = new RecipeSolution
            {
                Slots = new Dictionary<string, CardChoice> {
                    {"dream", new CardChoice { ElementID = "affliction"}},
                    {"medicine", new CardChoice { ElementID = "funds"}}
                }
            }
        };
    }
}