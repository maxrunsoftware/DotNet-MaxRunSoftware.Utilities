// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// https://en.wikipedia.org/wiki/ANSI_escape_code#3-bit_and_4-bit
/// </summary>
public partial class TerminalColor
{
    // @formatter:off

    public static TerminalColor4 Black               {get;}=new(" 30   40    0  rgb(0,0,0)        #000000  black                  ");
    public static TerminalColor4 Red                 {get;}=new(" 31   41    1  rgb(128,0,0)      #800000  red                    ");
    public static TerminalColor4 Green               {get;}=new(" 32   42    2  rgb(0,128,0)      #008000  green                  ");
    public static TerminalColor4 Yellow              {get;}=new(" 33   43    3  rgb(128,128,0)    #808000  yellow                 ");
    public static TerminalColor4 Blue                {get;}=new(" 34   44    4  rgb(0,0,128)      #000080  blue                   ");
    public static TerminalColor4 Magenta             {get;}=new(" 35   45    5  rgb(128,0,128)    #800080  magenta                ");
    public static TerminalColor4 Cyan                {get;}=new(" 36   46    6  rgb(0,128,128)    #008080  cyan                   ");
    public static TerminalColor4 White               {get;}=new(" 37   47    7  rgb(192,192,192)  #c0c0c0  white                  ");
    public static TerminalColor4 BrightBlack         {get;}=new(" 90  100    8  rgb(128,128,128)  #808080  bright_black           ");
    public static TerminalColor4 BrightRed           {get;}=new(" 91  101    9  rgb(255,0,0)      #ff0000  bright_red             ");
    public static TerminalColor4 BrightGreen         {get;}=new(" 92  102   10  rgb(0,255,0)      #00ff00  bright_green           ");
    public static TerminalColor4 BrightYellow        {get;}=new(" 93  103   11  rgb(255,255,0)    #ffff00  bright_yellow          ");
    public static TerminalColor4 BrightBlue          {get;}=new(" 94  104   12  rgb(0,0,255)      #0000ff  bright_blue            ");
    public static TerminalColor4 BrightMagenta       {get;}=new(" 95  105   13  rgb(255,0,255)    #ff00ff  bright_magenta         ");
    public static TerminalColor4 BrightCyan          {get;}=new(" 96  106   14  rgb(0,255,255)    #00ffff  bright_cyan            ");
    public static TerminalColor4 BrightWhite         {get;}=new(" 97  107   15  rgb(255,255,255)  #ffffff  bright_white           ");
    public static TerminalColor  Grey0               {get;}=new("           16  rgb(0,0,0)        #000000  grey0                  ");
    public static TerminalColor  NavyBlue            {get;}=new("           17  rgb(0,0,95)       #00005f  navy_blue              ");
    public static TerminalColor  DarkBlue            {get;}=new("           18  rgb(0,0,135)      #000087  dark_blue              ");
    public static TerminalColor  Blue3               {get;}=new("           20  rgb(0,0,215)      #0000d7  blue3                  ");
    public static TerminalColor  Blue1               {get;}=new("           21  rgb(0,0,255)      #0000ff  blue1                  ");
    public static TerminalColor  DarkGreen           {get;}=new("           22  rgb(0,95,0)       #005f00  dark_green             ");
    public static TerminalColor  DeepSkyBlue4        {get;}=new("           25  rgb(0,95,175)     #005faf  deep_sky_blue4         ");
    public static TerminalColor  DodgerBlue3         {get;}=new("           26  rgb(0,95,215)     #005fd7  dodger_blue3           ");
    public static TerminalColor  DodgerBlue2         {get;}=new("           27  rgb(0,95,255)     #005fff  dodger_blue2           ");
    public static TerminalColor  Green4              {get;}=new("           28  rgb(0,135,0)      #008700  green4                 ");
    public static TerminalColor  SpringGreen4        {get;}=new("           29  rgb(0,135,95)     #00875f  spring_green4          ");
    public static TerminalColor  Turquoise4          {get;}=new("           30  rgb(0,135,135)    #008787  turquoise4             ");
    public static TerminalColor  DeepSkyBlue3        {get;}=new("           32  rgb(0,135,215)    #0087d7  deep_sky_blue3         ");
    public static TerminalColor  DodgerBlue1         {get;}=new("           33  rgb(0,135,255)    #0087ff  dodger_blue1           ");
    public static TerminalColor  DarkCyan            {get;}=new("           36  rgb(0,175,135)    #00af87  dark_cyan              ");
    public static TerminalColor  LightSeaGreen       {get;}=new("           37  rgb(0,175,175)    #00afaf  light_sea_green        ");
    public static TerminalColor  DeepSkyBlue2        {get;}=new("           38  rgb(0,175,215)    #00afd7  deep_sky_blue2         ");
    public static TerminalColor  DeepSkyBlue1        {get;}=new("           39  rgb(0,175,255)    #00afff  deep_sky_blue1         ");
    public static TerminalColor  Green3              {get;}=new("           40  rgb(0,215,0)      #00d700  green3                 ");
    public static TerminalColor  SpringGreen3        {get;}=new("           41  rgb(0,215,95)     #00d75f  spring_green3          ");
    public static TerminalColor  Cyan3               {get;}=new("           43  rgb(0,215,175)    #00d7af  cyan3                  ");
    public static TerminalColor  DarkTurquoise       {get;}=new("           44  rgb(0,215,215)    #00d7d7  dark_turquoise         ");
    public static TerminalColor  Turquoise2          {get;}=new("           45  rgb(0,215,255)    #00d7ff  turquoise2             ");
    public static TerminalColor  Green1              {get;}=new("           46  rgb(0,255,0)      #00ff00  green1                 ");
    public static TerminalColor  SpringGreen2        {get;}=new("           47  rgb(0,255,95)     #00ff5f  spring_green2          ");
    public static TerminalColor  SpringGreen1        {get;}=new("           48  rgb(0,255,135)    #00ff87  spring_green1          ");
    public static TerminalColor  MediumSpringGreen   {get;}=new("           49  rgb(0,255,175)    #00ffaf  medium_spring_green    ");
    public static TerminalColor  Cyan2               {get;}=new("           50  rgb(0,255,215)    #00ffd7  cyan2                  ");
    public static TerminalColor  Cyan1               {get;}=new("           51  rgb(0,255,255)    #00ffff  cyan1                  ");
    public static TerminalColor  Purple4             {get;}=new("           55  rgb(95,0,175)     #5f00af  purple4                ");
    public static TerminalColor  Purple3             {get;}=new("           56  rgb(95,0,215)     #5f00d7  purple3                ");
    public static TerminalColor  BlueViolet          {get;}=new("           57  rgb(95,0,255)     #5f00ff  blue_violet            ");
    public static TerminalColor  Grey37              {get;}=new("           59  rgb(95,95,95)     #5f5f5f  grey37                 ");
    public static TerminalColor  MediumPurple4       {get;}=new("           60  rgb(95,95,135)    #5f5f87  medium_purple4         ");
    public static TerminalColor  SlateBlue3          {get;}=new("           62  rgb(95,95,215)    #5f5fd7  slate_blue3            ");
    public static TerminalColor  RoyalBlue1          {get;}=new("           63  rgb(95,95,255)    #5f5fff  royal_blue1            ");
    public static TerminalColor  Chartreuse4         {get;}=new("           64  rgb(95,135,0)     #5f8700  chartreuse4            ");
    public static TerminalColor  PaleTurquoise4      {get;}=new("           66  rgb(95,135,135)   #5f8787  pale_turquoise4        ");
    public static TerminalColor  SteelBlue           {get;}=new("           67  rgb(95,135,175)   #5f87af  steel_blue             ");
    public static TerminalColor  SteelBlue3          {get;}=new("           68  rgb(95,135,215)   #5f87d7  steel_blue3            ");
    public static TerminalColor  CornflowerBlue      {get;}=new("           69  rgb(95,135,255)   #5f87ff  cornflower_blue        ");
    public static TerminalColor  DarkSeaGreen4       {get;}=new("           71  rgb(95,175,95)    #5faf5f  dark_sea_green4        ");
    public static TerminalColor  CadetBlue           {get;}=new("           73  rgb(95,175,175)   #5fafaf  cadet_blue             ");
    public static TerminalColor  SkyBlue3            {get;}=new("           74  rgb(95,175,215)   #5fafd7  sky_blue3              ");
    public static TerminalColor  Chartreuse3         {get;}=new("           76  rgb(95,215,0)     #5fd700  chartreuse3            ");
    public static TerminalColor  SeaGreen3           {get;}=new("           78  rgb(95,215,135)   #5fd787  sea_green3             ");
    public static TerminalColor  Aquamarine3         {get;}=new("           79  rgb(95,215,175)   #5fd7af  aquamarine3            ");
    public static TerminalColor  MediumTurquoise     {get;}=new("           80  rgb(95,215,215)   #5fd7d7  medium_turquoise       ");
    public static TerminalColor  SteelBlue1          {get;}=new("           81  rgb(95,215,255)   #5fd7ff  steel_blue1            ");
    public static TerminalColor  SeaGreen2           {get;}=new("           83  rgb(95,255,95)    #5fff5f  sea_green2             ");
    public static TerminalColor  SeaGreen1           {get;}=new("           85  rgb(95,255,175)   #5fffaf  sea_green1             ");
    public static TerminalColor  DarkSlateGray2      {get;}=new("           87  rgb(95,255,255)   #5fffff  dark_slate_gray2       ");
    public static TerminalColor  DarkRed             {get;}=new("           88  rgb(135,0,0)      #870000  dark_red               ");
    public static TerminalColor  DarkMagenta         {get;}=new("           91  rgb(135,0,175)    #8700af  dark_magenta           ");
    public static TerminalColor  Orange4             {get;}=new("           94  rgb(135,95,0)     #875f00  orange4                ");
    public static TerminalColor  LightPink4          {get;}=new("           95  rgb(135,95,95)    #875f5f  light_pink4            ");
    public static TerminalColor  Plum4               {get;}=new("           96  rgb(135,95,135)   #875f87  plum4                  ");
    public static TerminalColor  MediumPurple3       {get;}=new("           98  rgb(135,95,215)   #875fd7  medium_purple3         ");
    public static TerminalColor  SlateBlue1          {get;}=new("           99  rgb(135,95,255)   #875fff  slate_blue1            ");
    public static TerminalColor  Wheat4              {get;}=new("          101  rgb(135,135,95)   #87875f  wheat4                 ");
    public static TerminalColor  Grey53              {get;}=new("          102  rgb(135,135,135)  #878787  grey53                 ");
    public static TerminalColor  LightSlateGrey      {get;}=new("          103  rgb(135,135,175)  #8787af  light_slate_grey       ");
    public static TerminalColor  MediumPurple        {get;}=new("          104  rgb(135,135,215)  #8787d7  medium_purple          ");
    public static TerminalColor  LightSlateBlue      {get;}=new("          105  rgb(135,135,255)  #8787ff  light_slate_blue       ");
    public static TerminalColor  Yellow4             {get;}=new("          106  rgb(135,175,0)    #87af00  yellow4                ");
    public static TerminalColor  DarkSeaGreen        {get;}=new("          108  rgb(135,175,135)  #87af87  dark_sea_green         ");
    public static TerminalColor  LightSkyBlue3       {get;}=new("          110  rgb(135,175,215)  #87afd7  light_sky_blue3        ");
    public static TerminalColor  SkyBlue2            {get;}=new("          111  rgb(135,175,255)  #87afff  sky_blue2              ");
    public static TerminalColor  Chartreuse2         {get;}=new("          112  rgb(135,215,0)    #87d700  chartreuse2            ");
    public static TerminalColor  PaleGreen3          {get;}=new("          114  rgb(135,215,135)  #87d787  pale_green3            ");
    public static TerminalColor  DarkSlateGray3      {get;}=new("          116  rgb(135,215,215)  #87d7d7  dark_slate_gray3       ");
    public static TerminalColor  SkyBlue1            {get;}=new("          117  rgb(135,215,255)  #87d7ff  sky_blue1              ");
    public static TerminalColor  Chartreuse1         {get;}=new("          118  rgb(135,255,0)    #87ff00  chartreuse1            ");
    public static TerminalColor  LightGreen          {get;}=new("          120  rgb(135,255,135)  #87ff87  light_green            ");
    public static TerminalColor  Aquamarine1         {get;}=new("          122  rgb(135,255,215)  #87ffd7  aquamarine1            ");
    public static TerminalColor  DarkSlateGray1      {get;}=new("          123  rgb(135,255,255)  #87ffff  dark_slate_gray1       ");
    public static TerminalColor  DeepPink4           {get;}=new("          125  rgb(175,0,95)     #af005f  deep_pink4             ");
    public static TerminalColor  MediumVioletRed     {get;}=new("          126  rgb(175,0,135)    #af0087  medium_violet_red      ");
    public static TerminalColor  DarkViolet          {get;}=new("          128  rgb(175,0,215)    #af00d7  dark_violet            ");
    public static TerminalColor  Purple              {get;}=new("          129  rgb(175,0,255)    #af00ff  purple                 ");
    public static TerminalColor  MediumOrchid3       {get;}=new("          133  rgb(175,95,175)   #af5faf  medium_orchid3         ");
    public static TerminalColor  MediumOrchid        {get;}=new("          134  rgb(175,95,215)   #af5fd7  medium_orchid          ");
    public static TerminalColor  DarkGoldenrod       {get;}=new("          136  rgb(175,135,0)    #af8700  dark_goldenrod         ");
    public static TerminalColor  RosyBrown           {get;}=new("          138  rgb(175,135,135)  #af8787  rosy_brown             ");
    public static TerminalColor  Grey63              {get;}=new("          139  rgb(175,135,175)  #af87af  grey63                 ");
    public static TerminalColor  MediumPurple2       {get;}=new("          140  rgb(175,135,215)  #af87d7  medium_purple2         ");
    public static TerminalColor  MediumPurple1       {get;}=new("          141  rgb(175,135,255)  #af87ff  medium_purple1         ");
    public static TerminalColor  DarkKhaki           {get;}=new("          143  rgb(175,175,95)   #afaf5f  dark_khaki             ");
    public static TerminalColor  NavajoWhite3        {get;}=new("          144  rgb(175,175,135)  #afaf87  navajo_white3          ");
    public static TerminalColor  Grey69              {get;}=new("          145  rgb(175,175,175)  #afafaf  grey69                 ");
    public static TerminalColor  LightSteelBlue3     {get;}=new("          146  rgb(175,175,215)  #afafd7  light_steel_blue3      ");
    public static TerminalColor  LightSteelBlue      {get;}=new("          147  rgb(175,175,255)  #afafff  light_steel_blue       ");
    public static TerminalColor  DarkOliveGreen3     {get;}=new("          149  rgb(175,215,95)   #afd75f  dark_olive_green3      ");
    public static TerminalColor  DarkSeaGreen3       {get;}=new("          150  rgb(175,215,135)  #afd787  dark_sea_green3        ");
    public static TerminalColor  LightCyan3          {get;}=new("          152  rgb(175,215,215)  #afd7d7  light_cyan3            ");
    public static TerminalColor  LightSkyBlue1       {get;}=new("          153  rgb(175,215,255)  #afd7ff  light_sky_blue1        ");
    public static TerminalColor  GreenYellow         {get;}=new("          154  rgb(175,255,0)    #afff00  green_yellow           ");
    public static TerminalColor  DarkOliveGreen2     {get;}=new("          155  rgb(175,255,95)   #afff5f  dark_olive_green2      ");
    public static TerminalColor  PaleGreen1          {get;}=new("          156  rgb(175,255,135)  #afff87  pale_green1            ");
    public static TerminalColor  DarkSeaGreen2       {get;}=new("          157  rgb(175,255,175)  #afffaf  dark_sea_green2        ");
    public static TerminalColor  PaleTurquoise1      {get;}=new("          159  rgb(175,255,255)  #afffff  pale_turquoise1        ");
    public static TerminalColor  Red3                {get;}=new("          160  rgb(215,0,0)      #d70000  red3                   ");
    public static TerminalColor  DeepPink3           {get;}=new("          162  rgb(215,0,135)    #d70087  deep_pink3             ");
    public static TerminalColor  Magenta3            {get;}=new("          164  rgb(215,0,215)    #d700d7  magenta3               ");
    public static TerminalColor  DarkOrange3         {get;}=new("          166  rgb(215,95,0)     #d75f00  dark_orange3           ");
    public static TerminalColor  IndianRed           {get;}=new("          167  rgb(215,95,95)    #d75f5f  indian_red             ");
    public static TerminalColor  HotPink3            {get;}=new("          168  rgb(215,95,135)   #d75f87  hot_pink3              ");
    public static TerminalColor  HotPink2            {get;}=new("          169  rgb(215,95,175)   #d75faf  hot_pink2              ");
    public static TerminalColor  Orchid              {get;}=new("          170  rgb(215,95,215)   #d75fd7  orchid                 ");
    public static TerminalColor  Orange3             {get;}=new("          172  rgb(215,135,0)    #d78700  orange3                ");
    public static TerminalColor  LightSalmon3        {get;}=new("          173  rgb(215,135,95)   #d7875f  light_salmon3          ");
    public static TerminalColor  LightPink3          {get;}=new("          174  rgb(215,135,135)  #d78787  light_pink3            ");
    public static TerminalColor  Pink3               {get;}=new("          175  rgb(215,135,175)  #d787af  pink3                  ");
    public static TerminalColor  Plum3               {get;}=new("          176  rgb(215,135,215)  #d787d7  plum3                  ");
    public static TerminalColor  Violet              {get;}=new("          177  rgb(215,135,255)  #d787ff  violet                 ");
    public static TerminalColor  Gold3               {get;}=new("          178  rgb(215,175,0)    #d7af00  gold3                  ");
    public static TerminalColor  LightGoldenrod3     {get;}=new("          179  rgb(215,175,95)   #d7af5f  light_goldenrod3       ");
    public static TerminalColor  Tan                 {get;}=new("          180  rgb(215,175,135)  #d7af87  tan                    ");
    public static TerminalColor  MistyRose3          {get;}=new("          181  rgb(215,175,175)  #d7afaf  misty_rose3            ");
    public static TerminalColor  Thistle3            {get;}=new("          182  rgb(215,175,215)  #d7afd7  thistle3               ");
    public static TerminalColor  Plum2               {get;}=new("          183  rgb(215,175,255)  #d7afff  plum2                  ");
    public static TerminalColor  Yellow3             {get;}=new("          184  rgb(215,215,0)    #d7d700  yellow3                ");
    public static TerminalColor  Khaki3              {get;}=new("          185  rgb(215,215,95)   #d7d75f  khaki3                 ");
    public static TerminalColor  LightYellow3        {get;}=new("          187  rgb(215,215,175)  #d7d7af  light_yellow3          ");
    public static TerminalColor  Grey84              {get;}=new("          188  rgb(215,215,215)  #d7d7d7  grey84                 ");
    public static TerminalColor  LightSteelBlue1     {get;}=new("          189  rgb(215,215,255)  #d7d7ff  light_steel_blue1      ");
    public static TerminalColor  Yellow2             {get;}=new("          190  rgb(215,255,0)    #d7ff00  yellow2                ");
    public static TerminalColor  DarkOliveGreen1     {get;}=new("          192  rgb(215,255,135)  #d7ff87  dark_olive_green1      ");
    public static TerminalColor  DarkSeaGreen1       {get;}=new("          193  rgb(215,255,175)  #d7ffaf  dark_sea_green1        ");
    public static TerminalColor  Honeydew2           {get;}=new("          194  rgb(215,255,215)  #d7ffd7  honeydew2              ");
    public static TerminalColor  LightCyan1          {get;}=new("          195  rgb(215,255,255)  #d7ffff  light_cyan1            ");
    public static TerminalColor  Red1                {get;}=new("          196  rgb(255,0,0)      #ff0000  red1                   ");
    public static TerminalColor  DeepPink2           {get;}=new("          197  rgb(255,0,95)     #ff005f  deep_pink2             ");
    public static TerminalColor  DeepPink1           {get;}=new("          199  rgb(255,0,175)    #ff00af  deep_pink1             ");
    public static TerminalColor  Magenta2            {get;}=new("          200  rgb(255,0,215)    #ff00d7  magenta2               ");
    public static TerminalColor  Magenta1            {get;}=new("          201  rgb(255,0,255)    #ff00ff  magenta1               ");
    public static TerminalColor  OrangeRed1          {get;}=new("          202  rgb(255,95,0)     #ff5f00  orange_red1            ");
    public static TerminalColor  IndianRed1          {get;}=new("          204  rgb(255,95,135)   #ff5f87  indian_red1            ");
    public static TerminalColor  HotPink             {get;}=new("          206  rgb(255,95,215)   #ff5fd7  hot_pink               ");
    public static TerminalColor  MediumOrchid1       {get;}=new("          207  rgb(255,95,255)   #ff5fff  medium_orchid1         ");
    public static TerminalColor  DarkOrange          {get;}=new("          208  rgb(255,135,0)    #ff8700  dark_orange            ");
    public static TerminalColor  Salmon1             {get;}=new("          209  rgb(255,135,95)   #ff875f  salmon1                ");
    public static TerminalColor  LightCoral          {get;}=new("          210  rgb(255,135,135)  #ff8787  light_coral            ");
    public static TerminalColor  PaleVioletRed1      {get;}=new("          211  rgb(255,135,175)  #ff87af  pale_violet_red1       ");
    public static TerminalColor  Orchid2             {get;}=new("          212  rgb(255,135,215)  #ff87d7  orchid2                ");
    public static TerminalColor  Orchid1             {get;}=new("          213  rgb(255,135,255)  #ff87ff  orchid1                ");
    public static TerminalColor  Orange1             {get;}=new("          214  rgb(255,175,0)    #ffaf00  orange1                ");
    public static TerminalColor  SandyBrown          {get;}=new("          215  rgb(255,175,95)   #ffaf5f  sandy_brown            ");
    public static TerminalColor  LightSalmon1        {get;}=new("          216  rgb(255,175,135)  #ffaf87  light_salmon1          ");
    public static TerminalColor  LightPink1          {get;}=new("          217  rgb(255,175,175)  #ffafaf  light_pink1            ");
    public static TerminalColor  Pink1               {get;}=new("          218  rgb(255,175,215)  #ffafd7  pink1                  ");
    public static TerminalColor  Plum1               {get;}=new("          219  rgb(255,175,255)  #ffafff  plum1                  ");
    public static TerminalColor  Gold1               {get;}=new("          220  rgb(255,215,0)    #ffd700  gold1                  ");
    public static TerminalColor  LightGoldenrod2     {get;}=new("          222  rgb(255,215,135)  #ffd787  light_goldenrod2       ");
    public static TerminalColor  NavajoWhite1        {get;}=new("          223  rgb(255,215,175)  #ffd7af  navajo_white1          ");
    public static TerminalColor  MistyRose1          {get;}=new("          224  rgb(255,215,215)  #ffd7d7  misty_rose1            ");
    public static TerminalColor  Thistle1            {get;}=new("          225  rgb(255,215,255)  #ffd7ff  thistle1               ");
    public static TerminalColor  Yellow1             {get;}=new("          226  rgb(255,255,0)    #ffff00  yellow1                ");
    public static TerminalColor  LightGoldenrod1     {get;}=new("          227  rgb(255,255,95)   #ffff5f  light_goldenrod1       ");
    public static TerminalColor  Khaki1              {get;}=new("          228  rgb(255,255,135)  #ffff87  khaki1                 ");
    public static TerminalColor  Wheat1              {get;}=new("          229  rgb(255,255,175)  #ffffaf  wheat1                 ");
    public static TerminalColor  Cornsilk1           {get;}=new("          230  rgb(255,255,215)  #ffffd7  cornsilk1              ");
    public static TerminalColor  Grey100             {get;}=new("          231  rgb(255,255,255)  #ffffff  grey100                ");
    public static TerminalColor  Grey3               {get;}=new("          232  rgb(8,8,8)        #080808  grey3                  ");
    public static TerminalColor  Grey7               {get;}=new("          233  rgb(18,18,18)     #121212  grey7                  ");
    public static TerminalColor  Grey11              {get;}=new("          234  rgb(28,28,28)     #1c1c1c  grey11                 ");
    public static TerminalColor  Grey15              {get;}=new("          235  rgb(38,38,38)     #262626  grey15                 ");
    public static TerminalColor  Grey19              {get;}=new("          236  rgb(48,48,48)     #303030  grey19                 ");
    public static TerminalColor  Grey23              {get;}=new("          237  rgb(58,58,58)     #3a3a3a  grey23                 ");
    public static TerminalColor  Grey27              {get;}=new("          238  rgb(68,68,68)     #444444  grey27                 ");
    public static TerminalColor  Grey30              {get;}=new("          239  rgb(78,78,78)     #4e4e4e  grey30                 ");
    public static TerminalColor  Grey35              {get;}=new("          240  rgb(88,88,88)     #585858  grey35                 ");
    public static TerminalColor  Grey39              {get;}=new("          241  rgb(98,98,98)     #626262  grey39                 ");
    public static TerminalColor  Grey42              {get;}=new("          242  rgb(108,108,108)  #6c6c6c  grey42                 ");
    public static TerminalColor  Grey46              {get;}=new("          243  rgb(118,118,118)  #767676  grey46                 ");
    public static TerminalColor  Grey50              {get;}=new("          244  rgb(128,128,128)  #808080  grey50                 ");
    public static TerminalColor  Grey54              {get;}=new("          245  rgb(138,138,138)  #8a8a8a  grey54                 ");
    public static TerminalColor  Grey58              {get;}=new("          246  rgb(148,148,148)  #949494  grey58                 ");
    public static TerminalColor  Grey62              {get;}=new("          247  rgb(158,158,158)  #9e9e9e  grey62                 ");
    public static TerminalColor  Grey66              {get;}=new("          248  rgb(168,168,168)  #a8a8a8  grey66                 ");
    public static TerminalColor  Grey70              {get;}=new("          249  rgb(178,178,178)  #b2b2b2  grey70                 ");
    public static TerminalColor  Grey74              {get;}=new("          250  rgb(188,188,188)  #bcbcbc  grey74                 ");
    public static TerminalColor  Grey78              {get;}=new("          251  rgb(198,198,198)  #c6c6c6  grey78                 ");
    public static TerminalColor  Grey82              {get;}=new("          252  rgb(208,208,208)  #d0d0d0  grey82                 ");
    public static TerminalColor  Grey85              {get;}=new("          253  rgb(218,218,218)  #dadada  grey85                 ");
    public static TerminalColor  Grey89              {get;}=new("          254  rgb(228,228,228)  #e4e4e4  grey89                 ");
    public static TerminalColor  Grey93              {get;}=new("          255  rgb(238,238,238)  #eeeeee  grey93                 ");

    // @formatter:on
}
