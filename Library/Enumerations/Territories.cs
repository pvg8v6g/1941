using Library.Attributes;

namespace Library.Enumerations;

public enum Territories
{
    [TerritoryInfo("United_Kingdom", "United Kingdom", Faction.UnitedKingdom)]
    UnitedKingdom,

    [TerritoryInfo("Ireland", "Ireland", Faction.Neutral)]
    Ireland,

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

    [TerritoryInfo("Greece", "Greece", Faction.Germany)]
    Greece,

    [TerritoryInfo("Turkey", "Turkey", Faction.Neutral)]
    Turkey,

    [TerritoryInfo("Soviet_Union", "Soviet Union", Faction.Russia)]
    SovietUnion,

    [TerritoryInfo("Morocco", "Morocco", Faction.Germany)]
    Morocco,

    [TerritoryInfo("Western_Sahara", "Western Sahara", Faction.Neutral)]
    WesternSahara,

    [TerritoryInfo("Algeria", "Algeria", Faction.Germany)]
    Algeria,

    [TerritoryInfo("Tunisia", "Tunisia", Faction.Germany)]
    Tunisia,

    [TerritoryInfo("Libya", "Libya", Faction.Italy)]
    Libya,

    [TerritoryInfo("Egypt", "Egypt", Faction.UnitedKingdom)]
    Egypt,

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

    [TerritoryInfo("Sudan", "Sudan", Faction.UnitedKingdom)]
    Sudan,

    [TerritoryInfo("Eritrea", "Eritrea", Faction.UnitedKingdom)]
    Eritrea,

    [TerritoryInfo("Ethiopia", "Ethiopia", Faction.UnitedKingdom)]
    Ethiopia,

    [TerritoryInfo("Djibouti", "Djibouti", Faction.UnitedKingdom)]
    Djibouti,

    [TerritoryInfo("Somalia", "Somalia", Faction.UnitedKingdom)]
    Somalia,

    [TerritoryInfo("Senegal", "Senegal", Faction.Germany)]
    Senegal,

    [TerritoryInfo("The_Gambia", "The Gambia", Faction.UnitedKingdom)]
    TheGambia,

    [TerritoryInfo("Guinea_â_Bissau", "Guinea â Bissau", Faction.UnitedKingdom)]
    GuineaâBissau,

    [TerritoryInfo("Guinea", "Guinea", Faction.Germany)]
    Guinea,

    [TerritoryInfo("Sierra_Leone", "Sierra Leone", Faction.UnitedKingdom)]
    SierraLeone,

    [TerritoryInfo("Liberia", "Liberia", Faction.Neutral)]
    Liberia,

    [TerritoryInfo("CÃted_Ivoire", "CÃted Ivoire", Faction.Germany)]
    CÃtedIvoire,

    [TerritoryInfo("Ghana", "Ghana", Faction.UnitedKingdom)]
    Ghana,

    [TerritoryInfo("Togo", "Togo", Faction.Germany)]
    Togo,

    [TerritoryInfo("Benin", "Benin", Faction.Germany)]
    Benin,

    [TerritoryInfo("Burkina_Faso", "Burkina Faso", Faction.Germany)]
    BurkinaFaso,

    [TerritoryInfo("Mali", "Mali", Faction.Germany)]
    Mali,

    [TerritoryInfo("Mauritania", "Mauritania", Faction.Germany)]
    Mauritania,

    [TerritoryInfo("Niger", "Niger", Faction.Germany)]
    Niger,

    [TerritoryInfo("Central_African_Republic", "Central African Republic", Faction.Germany)]
    CentralAfricanRepublic,

    [TerritoryInfo("Cameroon", "Cameroon", Faction.Germany)]
    Cameroon,

    [TerritoryInfo("Equatorial_Guinea", "Equatorial Guinea", Faction.Neutral)]
    EquatorialGuinea,

    [TerritoryInfo("Gabon", "Gabon", Faction.Germany)]
    Gabon,

    [TerritoryInfo("Republic_of_the_Congo", "Republic of the Congo", Faction.Germany)]
    RepublicoftheCongo,

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

    [TerritoryInfo("Madagascar", "Madagascar", Faction.Germany)]
    Madagascar,

    [TerritoryInfo("Cape_Verde", "Cape Verde", Faction.UnitedKingdom)]
    CapeVerde,

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

    [TerritoryInfo("Mongolia", "Mongolia", Faction.Russia)]
    Mongolia,

    [TerritoryInfo("Korea", "Korea", Faction.Japan)]
    Korea,

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

    [TerritoryInfo("Hawaiian_Islands", "Hawaiian Islands", Faction.UnitedStates)]
    HawaiianIslands,

    [TerritoryInfo("Papua", "Papua", Faction.Australia)]
    Papua,

    [TerritoryInfo("Western_Canada", "Western Canada", Faction.UnitedKingdom)]
    WesternCanada,

    [TerritoryInfo("Eastern_Canada", "Eastern Canada", Faction.UnitedKingdom)]
    EasternCanada,

    [TerritoryInfo("Western_United_States", "Western United States", Faction.UnitedStates)]
    WesternUnitedStates,

    [TerritoryInfo("Alaska", "Alaska", Faction.UnitedStates)]
    Alaska,

    [TerritoryInfo("Eastern_United_States", "Eastern United States", Faction.UnitedStates)]
    EasternUnitedStates,

    [TerritoryInfo("Western_Mexico", "Western Mexico", Faction.Neutral)]
    WesternMexico,

    [TerritoryInfo("Eastern_Mexico", "Eastern Mexico", Faction.Neutral)]
    EasternMexico,

    [TerritoryInfo("Greenland", "Greenland", Faction.UnitedStates)]
    Greenland,

    [TerritoryInfo("Bermuda", "Bermuda", Faction.UnitedKingdom)]
    Bermuda,

    [TerritoryInfo("Guatemala", "Guatemala", Faction.Neutral)]
    Guatemala,

    [TerritoryInfo("Belize", "Belize", Faction.UnitedKingdom)]
    Belize,

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

    [TerritoryInfo("Jamaica", "Jamaica", Faction.UnitedKingdom)]
    Jamaica,

    [TerritoryInfo("Haiti", "Haiti", Faction.Neutral)]
    Haiti,

    [TerritoryInfo("Dominican_Republic", "Dominican Republic", Faction.Neutral)]
    DominicanRepublic,

    [TerritoryInfo("Puerto_Rico", "Puerto Rico", Faction.UnitedStates)]
    PuertoRico,

    [TerritoryInfo("Bahamas", "Bahamas", Faction.UnitedKingdom)]
    Bahamas,

    [TerritoryInfo("Trinidad_Tobago", "Trinidad Tobago", Faction.UnitedKingdom)]
    TrinidadTobago,

    [TerritoryInfo("French_Guiana", "French Guiana", Faction.Germany)]
    FrenchGuiana,

    [TerritoryInfo("Guyana", "Guyana", Faction.UnitedKingdom)]
    Guyana,

    [TerritoryInfo("Suriname", "Suriname", Faction.UnitedKingdom)]
    Suriname,

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

    [TerritoryInfo("Argentina", "Argentina", Faction.Neutral)]
    Argentina,

    [TerritoryInfo("Chile", "Chile", Faction.Neutral)]
    Chile,

    [TerritoryInfo("Uruguay", "Uruguay", Faction.Neutral)]
    Uruguay,

    [TerritoryInfo("Paraguay", "Paraguay", Faction.Neutral)]
    Paraguay,

    [TerritoryInfo("Brazil", "Brazil", Faction.Neutral)]
    Brazil,

    [TerritoryInfo("Falkland_Islands", "Falkland Islands", Faction.UnitedKingdom)]
    FalklandIslands
}
