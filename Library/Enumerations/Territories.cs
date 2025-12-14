using Library.Attributes;

namespace Library.Enumerations;

public enum Territories
{
    [TerritoryInfo("Afghanistan", "Afghanistan", Faction.Neutral)]
    Afghanistan,

    [TerritoryInfo("British_India", "British India", Faction.UnitedKingdom)]
    BritishIndia,

    [TerritoryInfo("Nepal", "Nepal", Faction.UnitedKingdom)]
    Nepal,

    [TerritoryInfo("Bhutan", "Bhutan", Faction.UnitedKingdom)]
    Bhutan,

    [TerritoryInfo("Ceylon", "Ceylon", Faction.UnitedKingdom)]
    Ceylon,

    [TerritoryInfo("China", "China", Faction.China)]
    China,

    // New East Asia splits inside historical China for 1941 scenario
    // These classes must match paths in Graphics/Images/world.svg
    [TerritoryInfo("Taiwan", "Taiwan", Faction.Japan)]
    Taiwan,

    // New East Asia splits inside historical China for 1941 scenario
    // These classes must match paths in Graphics/Images/world.svg
    [TerritoryInfo("Manchukuo", "Manchukuo", Faction.Japan)]
    Manchukuo,

    [TerritoryInfo("Taierzhuang", "Taierzhuang", Faction.Japan)]
    Taierzhuang,

    [TerritoryInfo("South_Korea", "South Korea", Faction.Japan)]
    SouthKorea,

    [TerritoryInfo("North_Korea", "North Korea", Faction.Japan)]
    NorthKorea,

    [TerritoryInfo("Formosa", "Formosa", Faction.Japan)]
    Formosa,

    [TerritoryInfo("Japan", "Japan", Faction.Japan)]
    Japan,

    [TerritoryInfo("Burma", "Burma", Faction.UnitedKingdom)]
    Burma,

    [TerritoryInfo("French_Indochina", "French Indochina", Faction.Japan)]
    FrenchIndochina,

    [TerritoryInfo("Siam", "Siam", Faction.Neutral)]
    Siam,

    [TerritoryInfo("British_Malaya", "British Malaya", Faction.UnitedKingdom)]
    BritishMalaya,

    [TerritoryInfo("Philippines", "Philippines", Faction.UnitedStates)]
    Philippines,

    [TerritoryInfo("Netherlands_East_Indies", "Netherlands East Indies", Faction.UnitedKingdom)]
    NetherlandsEastIndies,

    [TerritoryInfo("Hong_Kong", "Hong Kong", Faction.UnitedKingdom)]
    HongKong,

    [TerritoryInfo("Macau", "Macau", Faction.UnitedKingdom)]
    Macau,

    [TerritoryInfo("Australia", "Australia", Faction.Australia)]
    Australia,

    [TerritoryInfo("New_Zealand", "New Zealand", Faction.Australia)]
    NewZealand,

    [TerritoryInfo("New_Guinea", "New Guinea", Faction.Australia)]
    NewGuinea,

    [TerritoryInfo("Solomon_Islands", "Solomon Islands", Faction.Australia)]
    SolomonIslands,

    [TerritoryInfo("New_Caledonia", "New Caledonia", Faction.UnitedKingdom)]
    NewCaledonia,

    [TerritoryInfo("Fiji", "Fiji", Faction.Australia)]
    Fiji,

    [TerritoryInfo("Papua", "Papua", Faction.Australia)]
    Papua,

    #region Europe

    [TerritoryInfo("Czech_Republic", "Czech Republic", Faction.Germany)]
    CzechRepublic,

    [TerritoryInfo("Belarus", "Belarus", Faction.Germany)]
    Belarus,

    [TerritoryInfo("Ukraine", "Ukraine", Faction.Germany)]
    Ukraine,

    [TerritoryInfo("Slovakia", "Slovakia", Faction.Germany)]
    Slovakia,

    [TerritoryInfo("Moldova", "Moldova", Faction.Germany)]
    Moldova,

    [TerritoryInfo("Serbia", "Serbia", Faction.Germany)]
    Serbia,

    [TerritoryInfo("Macedonia", "Macedonia", Faction.Germany)]
    Macedonia,

    [TerritoryInfo("Kosovo", "Kosovo", Faction.Germany)]
    Kosovo,

    [TerritoryInfo("Montenegro", "Montenegro", Faction.Germany)]
    Montenegro,

    [TerritoryInfo("Bosnia_and_Herzegovina", "Bosnia And Herzegovina", Faction.Germany)]
    BosniaHerzegovina,

    [TerritoryInfo("Croatia", "Croatia", Faction.Germany)]
    Croatia,

    [TerritoryInfo("Slovenia", "Slovenia", Faction.Germany)]
    Slovenia,

    [TerritoryInfo("United_Kingdom", "United Kingdom", Faction.UnitedKingdom)]
    UnitedKingdom,

    [TerritoryInfo("Ireland", "Ireland", Faction.Neutral)]
    Ireland,

    [TerritoryInfo("Iceland", "Iceland", Faction.Neutral)]
    Iceland,

    [TerritoryInfo("France", "France", Faction.Germany)]
    France,

    [TerritoryInfo("Spain", "Spain", Faction.Neutral)]
    Spain,

    [TerritoryInfo("Portugal", "Portugal", Faction.UnitedKingdom)]
    Portugal,

    [TerritoryInfo("Belgium", "Belgium", Faction.UnitedKingdom)]
    Belgium,

    [TerritoryInfo("Netherlands", "Netherlands", Faction.UnitedKingdom)]
    Netherlands,

    [TerritoryInfo("Luxembourg", "Luxembourg", Faction.Germany)]
    Luxembourg,

    [TerritoryInfo("Switzerland", "Switzerland", Faction.Neutral)]
    Switzerland,

    [TerritoryInfo("Germany", "Germany", Faction.Germany)]
    Germany,

    [TerritoryInfo("Austria", "Austria", Faction.Germany)]
    Austria,

    [TerritoryInfo("Czechoslovakia", "Czechoslovakia", Faction.Germany)]
    Czechoslovakia,

    [TerritoryInfo("Hungary", "Hungary", Faction.Germany)]
    Hungary,

    [TerritoryInfo("Denmark", "Denmark", Faction.Germany)]
    Denmark,

    [TerritoryInfo("Norway", "Norway", Faction.Germany)]
    Norway,

    [TerritoryInfo("Sweden", "Sweden", Faction.Neutral)]
    Sweden,

    [TerritoryInfo("Finland", "Finland", Faction.Germany)]
    Finland,

    [TerritoryInfo("Estonia", "Estonia", Faction.Russia)]
    Estonia,

    [TerritoryInfo("Latvia", "Latvia", Faction.Russia)]
    Latvia,

    [TerritoryInfo("Lithuania", "Lithuania", Faction.Russia)]
    Lithuania,

    [TerritoryInfo("Poland", "Poland", Faction.Germany)]
    Poland,

    [TerritoryInfo("Yugoslavia", "Yugoslavia", Faction.Germany)]
    Yugoslavia,

    [TerritoryInfo("Romania", "Romania", Faction.Germany)]
    Romania,

    [TerritoryInfo("Bulgaria", "Bulgaria", Faction.Germany)]
    Bulgaria,

    [TerritoryInfo("Albania", "Albania", Faction.Italy)]
    Albania,

    [TerritoryInfo("Italy", "Italy", Faction.Italy)]
    Italy,

    [TerritoryInfo("Greece", "Greece", Faction.Germany)]
    Greece,

    #endregion

    #region Middle East

    [TerritoryInfo("Israel", "Israel", Faction.UnitedKingdom)]
    Israel,

    [TerritoryInfo("Jordan", "Jordan", Faction.UnitedKingdom)]
    Jordan,

    [TerritoryInfo("Turkey", "Turkey", Faction.Neutral)]
    Turkey,

    [TerritoryInfo("Syria", "Syria", Faction.Germany)]
    Syria,

    [TerritoryInfo("Lebanon", "Lebanon", Faction.Germany)]
    Lebanon,

    [TerritoryInfo("Palestine", "Palestine", Faction.UnitedKingdom)]
    Palestine,

    [TerritoryInfo("Transjordan", "Transjordan", Faction.UnitedKingdom)]
    Transjordan,

    [TerritoryInfo("Iraq", "Iraq", Faction.UnitedKingdom)]
    Iraq,

    [TerritoryInfo("Persia", "Persia", Faction.UnitedKingdom)]
    Persia,

    [TerritoryInfo("Kuwait", "Kuwait", Faction.UnitedKingdom)]
    Kuwait,

    [TerritoryInfo("Bahrain", "Bahrain", Faction.UnitedKingdom)]
    Bahrain,

    [TerritoryInfo("Qatar", "Qatar", Faction.UnitedKingdom)]
    Qatar,

    [TerritoryInfo("Trucial_States", "Trucial States", Faction.UnitedKingdom)]
    TrucialStates,

    [TerritoryInfo("Oman", "Oman", Faction.UnitedKingdom)]
    Oman,

    [TerritoryInfo("Yemen", "Yemen", Faction.Neutral)]
    Yemen,

    [TerritoryInfo("Saudi_Arabia", "Saudi Arabia", Faction.Neutral)]
    SaudiArabia,

    #endregion

    #region Africa

    [TerritoryInfo("Morocco", "Morocco", Faction.UnitedKingdom)]
    Morocco,

    [TerritoryInfo("Uganda", "Uganda", Faction.UnitedKingdom)]
    Uganda,

    [TerritoryInfo("Lesotho", "Lesotho", Faction.UnitedKingdom)]
    Lesotho,

    [TerritoryInfo("Rwanda", "Rwanda", Faction.UnitedKingdom)]
    Rwanda,

    [TerritoryInfo("Burundi", "Burundi", Faction.UnitedKingdom)]
    Burundi,

    [TerritoryInfo("Tanzania", "Tanzania", Faction.UnitedKingdom)]
    Tanzania,

    [TerritoryInfo("Kenya", "Kenya", Faction.UnitedKingdom)]
    Kenya,

    [TerritoryInfo("Chad", "Chad", Faction.UnitedKingdom)]
    Chad,

    [TerritoryInfo("Western_Sahara", "Western Sahara", Faction.UnitedKingdom)]
    WesternSahara,

    [TerritoryInfo("Algeria", "Algeria", Faction.UnitedKingdom)]
    Algeria,

    [TerritoryInfo("Tunisia", "Tunisia", Faction.UnitedKingdom)]
    Tunisia,

    [TerritoryInfo("Libya", "Libya", Faction.Italy)]
    Libya,

    [TerritoryInfo("Egypt", "Egypt", Faction.Neutral)]
    Egypt,

    [TerritoryInfo("Sudan", "Sudan", Faction.UnitedKingdom)]
    Sudan,

    [TerritoryInfo("Eritrea", "Eritrea", Faction.UnitedKingdom)]
    Eritrea,

    [TerritoryInfo("Ethiopia", "Ethiopia", Faction.Neutral)]
    Ethiopia,

    [TerritoryInfo("Djibouti", "Djibouti", Faction.UnitedKingdom)]
    Djibouti,

    [TerritoryInfo("Somalia", "Somalia", Faction.UnitedKingdom)]
    Somalia,

    [TerritoryInfo("Senegal", "Senegal", Faction.UnitedKingdom)]
    Senegal,

    [TerritoryInfo("The_Gambia", "The Gambia", Faction.UnitedKingdom)]
    TheGambia,

    [TerritoryInfo("Guinea-Bissau", "Guinea â Bissau", Faction.UnitedKingdom)]
    GuineaBissau,

    [TerritoryInfo("Guinea", "Guinea", Faction.UnitedKingdom)]
    Guinea,

    [TerritoryInfo("Sierra_Leone", "Sierra Leone", Faction.UnitedKingdom)]
    SierraLeone,

    [TerritoryInfo("Liberia", "Liberia", Faction.UnitedKingdom)]
    Liberia,

    [TerritoryInfo("Ivory_Coast", "Ivory Coast", Faction.UnitedKingdom)]
    IvoryCoast,

    [TerritoryInfo("Ghana", "Ghana", Faction.UnitedKingdom)]
    Ghana,

    [TerritoryInfo("Togo", "Togo", Faction.UnitedKingdom)]
    Togo,

    [TerritoryInfo("Benin", "Benin", Faction.UnitedKingdom)]
    Benin,

    [TerritoryInfo("Burkina_Faso", "Burkina Faso", Faction.UnitedKingdom)]
    BurkinaFaso,

    [TerritoryInfo("Mali", "Mali", Faction.UnitedKingdom)]
    Mali,

    [TerritoryInfo("Mauritania", "Mauritania", Faction.UnitedKingdom)]
    Mauritania,

    [TerritoryInfo("Niger", "Niger", Faction.UnitedKingdom)]
    Niger,

    [TerritoryInfo("Nigeria", "Nigeria", Faction.UnitedKingdom)]
    Nigeria,

    [TerritoryInfo("Central_African_Republic", "Central African Republic", Faction.UnitedKingdom)]
    CentralAfricanRepublic,

    [TerritoryInfo("Cameroon", "Cameroon", Faction.UnitedKingdom)]
    Cameroon,

    [TerritoryInfo("Equatorial_Guinea", "Equatorial Guinea", Faction.UnitedKingdom)]
    EquatorialGuinea,

    [TerritoryInfo("Gabon", "Gabon", Faction.UnitedKingdom)]
    Gabon,

    [TerritoryInfo("Republic_of_Congo", "Republic of Congo", Faction.UnitedKingdom)]
    RepublicOfCongo,

    [TerritoryInfo("Belgian_Congo", "Belgian Congo", Faction.UnitedKingdom)]
    BelgianCongo,

    [TerritoryInfo("Angola", "Angola", Faction.UnitedKingdom)]
    Angola,

    [TerritoryInfo("Zambia", "Zambia", Faction.UnitedKingdom)]
    Zambia,

    [TerritoryInfo("Malawi", "Malawi", Faction.UnitedKingdom)]
    Malawi,

    [TerritoryInfo("Mozambique", "Mozambique", Faction.UnitedKingdom)]
    Mozambique,

    [TerritoryInfo("Namibia", "Namibia", Faction.UnitedKingdom)]
    Namibia,

    [TerritoryInfo("Botswana", "Botswana", Faction.UnitedKingdom)]
    Botswana,

    [TerritoryInfo("Zimbabwe", "Zimbabwe", Faction.UnitedKingdom)]
    Zimbabwe,

    [TerritoryInfo("South_Africa", "South Africa", Faction.UnitedKingdom)]
    SouthAfrica,

    [TerritoryInfo("Swaziland", "Swaziland", Faction.UnitedKingdom)]
    Swaziland,

    [TerritoryInfo("Madagascar", "Madagascar", Faction.UnitedKingdom)]
    Madagascar,

    [TerritoryInfo("Cape_Verde", "Cape Verde", Faction.UnitedKingdom)]
    CapeVerde,

    #endregion

    #region Russian Federation

    [TerritoryInfo("Soviet_Union", "Soviet Union", Faction.Russia)]
    SovietUnion,

    [TerritoryInfo("Mongolia", "Mongolia", Faction.Neutral)]
    Mongolia,

    #endregion

    #region Eastern North America

    [TerritoryInfo("Eastern_Canada", "Eastern Canada", Faction.UnitedKingdom)]
    EasternCanada,

    [TerritoryInfo("Eastern_United_States", "Eastern United States", Faction.UnitedStates)]
    EasternUnitedStates,

    [TerritoryInfo("Eastern_Mexico", "Eastern Mexico", Faction.UnitedStates)]
    EasternMexico,

    [TerritoryInfo("Greenland", "Greenland", Faction.UnitedStates)]
    Greenland,

    #endregion

    #region Central America

    [TerritoryInfo("El_Salvador", "El Salvador", Faction.Neutral)]
    ElSalvador,

    [TerritoryInfo("Honduras", "Honduras", Faction.Neutral)]
    Honduras,

    [TerritoryInfo("Nicaragua", "Nicaragua", Faction.Neutral)]
    Nicaragua,

    [TerritoryInfo("Costa_Rica", "Costa Rica", Faction.Neutral)]
    CostaRica,

    [TerritoryInfo("Panama", "Panama", Faction.Neutral)]
    Panama,

    [TerritoryInfo("Cuba", "Cuba", Faction.Neutral)]
    Cuba,

    [TerritoryInfo("Jamaica", "Jamaica", Faction.Neutral)]
    Jamaica,

    [TerritoryInfo("Haiti", "Haiti", Faction.Neutral)]
    Haiti,

    [TerritoryInfo("Dominican_Republic", "Dominican Republic", Faction.Neutral)]
    DominicanRepublic,

    [TerritoryInfo("Puerto_Rico", "Puerto Rico", Faction.Neutral)]
    PuertoRico,

    [TerritoryInfo("Bahamas", "Bahamas", Faction.Neutral)]
    Bahamas,

    [TerritoryInfo("Bermuda", "Bermuda", Faction.Neutral)]
    Bermuda,

    [TerritoryInfo("Guatemala", "Guatemala", Faction.Neutral)]
    Guatemala,

    [TerritoryInfo("Belize", "Belize", Faction.Neutral)]
    Belize,

    #endregion

    #region South America

    [TerritoryInfo("Brazil", "Brazil", Faction.UnitedStates)]
    Brazil,

    [TerritoryInfo("Argentina", "Argentina", Faction.Neutral)]
    Argentina,

    [TerritoryInfo("Chile", "Chile", Faction.Neutral)]
    Chile,

    [TerritoryInfo("Venezuela", "Venezuela", Faction.Neutral)]
    Venezuela,

    [TerritoryInfo("Colombia", "Colombia", Faction.Neutral)]
    Colombia,

    [TerritoryInfo("Ecuador", "Ecuador", Faction.Neutral)]
    Ecuador,

    [TerritoryInfo("Peru", "Peru", Faction.Neutral)]
    Peru,

    [TerritoryInfo("Bolivia", "Bolivia", Faction.Neutral)]
    Bolivia,

    [TerritoryInfo("Uruguay", "Uruguay", Faction.Neutral)]
    Uruguay,

    [TerritoryInfo("Paraguay", "Paraguay", Faction.Neutral)]
    Paraguay,

    [TerritoryInfo("Falkland_Islands", "Falkland Islands", Faction.Neutral)]
    FalklandIslands,

    [TerritoryInfo("Trinidad_Tobago", "Trinidad Tobago", Faction.Neutral)]
    TrinidadTobago,

    [TerritoryInfo("French_Guiana", "French Guiana", Faction.Neutral)]
    FrenchGuiana,

    [TerritoryInfo("Guyana", "Guyana", Faction.Neutral)]
    Guyana,

    [TerritoryInfo("Suriname", "Suriname", Faction.Neutral)]
    Suriname,

    #endregion

    #region Western North America

    [TerritoryInfo("Western_Canada", "Western Canada", Faction.UnitedKingdom)]
    WesternCanada,

    [TerritoryInfo("Western_United_States", "Western United States", Faction.UnitedStates)]
    WesternUnitedStates,

    [TerritoryInfo("Alaska", "Alaska", Faction.UnitedStates)]
    Alaska,

    [TerritoryInfo("Hawaii", "Hawaii", Faction.UnitedStates)]
    Hawaii,

    [TerritoryInfo("Western_Mexico", "Western Mexico", Faction.UnitedStates)]
    WesternMexico,

    [TerritoryInfo("French_Polynesia", "French Polynesia", Faction.UnitedStates)]
    FrenchPolynesia,

    #endregion
}
