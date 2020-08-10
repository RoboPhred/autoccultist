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
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "introjob"}
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "bequestintro"}
                                },
                            },
                            Imperatives = new List<Imperative> {
                                new Imperative {
                                    Name = "Work the intro job",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "work",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementId = "introjob"}}
                                        }
                                    },
                                }
                            }
                        },
                        new Goal {
                            Name = "Study Bequest",
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "bequestintro"}
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "ascensionenlightenmenta"}
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
                                            {"study", new CardChoice { ElementId = "bequestintro"}},
                                            {"Approach", new CardChoice { ElementId = "reason"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Find a Collaborator",
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "contactintro"}
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
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
                                            {"study", new CardChoice { ElementId = "contactintro"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Get Swole 1",
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "health"}
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AnyOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "skillhealtha"},
                                    new CardChoice { ElementId = "skillhealthb"},
                                    new CardChoice { ElementId = "skillhealthc"}
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
                                            {"study", new CardChoice{ElementId = "vitality"}},
                                            {"morevitality", new CardChoice {ElementId = "vitality"}}
                                        }
                                    }
                                },
                                RefreshHealth
                            }
                        },
                        new Goal {
                            Name = "Get Swole 2",
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "skillhealtha"}
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AnyOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "skillhealthb"},
                                    new CardChoice { ElementId = "skillhealthc"}
                                }
                            },
                            Imperatives = new List<Imperative> {
                                RefreshHealth,
                                new Imperative {
                                    Name = "Excersize",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            { "study", new CardChoice{ElementId = "health"}}
                                        }
                                    }
                                },
                                PaintAwayRestlessness,
                                HealtAfflictionWithFunds,
                                new Imperative {
                                    Name = "Learn a lesson from Vitality",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice{ElementId = "vitality"}},
                                            {"morevitality", new CardChoice {ElementId = "vitality"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Stronger Physique work",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "work",
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our skill card if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementId = "skillhealtha"}},
                                            {"Health", new CardChoice { ElementId = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Get Swole 2",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementId = "skillhealtha"}},
                                            {"V1", new CardChoice {ElementId="vitalityplus"}},
                                            {"V2", new CardChoice {ElementId="vitalityplus"}}
                                        }
                                    }
                                }
                            }
                        },
                        new Goal {
                            Name = "Get Swole 3",
                            RequiredCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "skillhealthb" }
                                }
                            },
                            CompletedByCards = new GameStateCondition {
                                AllOf = new List<CardChoice> {
                                    new CardChoice { ElementId = "skillhealthc" }
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
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            { "study", new CardChoice{ElementId = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Learn a lesson from Vitality",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our study verb if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice{ElementId = "vitality"}},
                                            {"morevitality", new CardChoice {ElementId = "vitality"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Hardened Physique work",
                                    Priority = ImperativePriority.Maintenance,
                                    Verb = "work",
                                    ForbidWhenCardsPresent = new GameStateCondition {
                                        AllOf = new List<CardChoice> {
                                            // Do not monopolize our skill card if we have two vitality waiting to be upgraded.
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                            new CardChoice { ElementId = "vitalityplus"},
                                        }
                                    },
                                    StartingRecipe = new RecipeSolution
                                    {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"work", new CardChoice { ElementId = "skillhealthb"}},
                                            {"Health", new CardChoice { ElementId = "health"}}
                                        }
                                    }
                                },
                                new Imperative {
                                    Name = "Get Swole 3",
                                    Priority = ImperativePriority.Goal,
                                    Verb = "study",
                                    StartingRecipe = new RecipeSolution {
                                        Slots = new Dictionary<string, CardChoice> {
                                            {"study", new CardChoice { ElementId = "skillhealthb"}},
                                            {"V1", new CardChoice {ElementId="vitalityplus"}},
                                            {"V2", new CardChoice {ElementId="vitalityplus"}},
                                            {"V3", new CardChoice {ElementId="vitalityplus"}}
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
                    {"work", new CardChoice { ElementId = "health"}}
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
                    {"dream", new CardChoice{ElementId = "fatigue"}}
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
                    {"work", new CardChoice {ElementId = "passion"}}
                }
            },
            OngoingRecipes = new Dictionary<string, RecipeSolution> {
                {"paintbasic", new RecipeSolution {
                    Slots = new Dictionary<string, CardChoice> {
                        {"Yearning", new CardChoice {ElementId = "restlessness"}}
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
                    {"dream", new CardChoice { ElementId = "affliction"}},
                    {"medicine", new CardChoice { ElementId = "funds"}}
                }
            }
        };
    }
}