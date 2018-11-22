# All Built-In Commands

As of Rocketbox 0.1, the built-in commands and relevant contents of the default Rocketbox database are as follows:

## Web Searching

Rocketbox can instantly open the system's default web browser to a search result page.

**Syntax**: *[keyword] [search terms]*

#### Keywords
Most of the search engines below have their entire name available as a keyword, but also have the following aliases:

- **Google**: *gl*, *goog*
- **Google (I'm Feeling Lucky)**: *ifl*, *lucky*
- **Google Images**: *img*, *gim*
- **Google Maps**: *maps*, *gmaps*
- **Amazon**: *az*
-- (outside of the United States) **Amazon USA/World**: *azus*, *azusa*
- **YouTube**: *yt*
- **Wikipedia**: *wp*
- **Reddit**: *reddit*
-- **Subreddit**: */r/*, *r/*, *sub*
- **Bing**: *bing*
- **Twitter**: *tw*, *#*
- **GitHub**: *gh*
- **Stack Overflow**: *so*
- **Steam**: *stm*
-- **Steam Community**: *sc*
- **Internet Archive**: *ia*, *intarc*
-- **Wayback Machine**: *wb*

## Calculator

Rocketbox can perform math on-the-fly.

**Syntax**: *= [expression]*

Output from the calculator can be copied to the clipboard with Enter.

## Unit Conversion

Rocketbox can convert between different units of measurement.

**Syntax**: *[keyword] [unit] [any number of words] [unit]*

**Keywords**: *convert*, *con*, *cv*, *c*

The following units are included, and can be referenced by various short forms including their official symbols.

- **Distance**: Millimeters, Centimeters, Inches, Feet, Yards, Meters, Kilometers, Miles
- **Volume**: Milliliters, Liters, US Fluid Ounces, US Pints, US Quarts, US Gallons
- **Mass**: Milligrams, Grams, Kilograms, Ounces, Pounds, Metric Tons, US Tons
- **Data**: Bits, Kilobits, Megabits, Gigabits, Bytes, Kilobytes, Megabytes, Gigabytes, Terabytes, Kibibytes, Mebibytes, Gibibytes, Tebibytes

## Time Calculation

Output from any of the following commands can be copied to the clipboard with Enter.

### Current Time

Rocketbox can display the current date/time down to the second using the following keywords:

- **Standard date/time**: *t*, *time*
- **Unix time**: *ut*

### Add/Subtract Time

Rocketbox can add or subtract units of time from the current date/time.

**Syntax**: *[keyword] [any number of values]*

**Keywords**: *t+* to add, and *t-* to subtract.

Values of time must be expressed as a number immediately followed by the unit, without a space between them. Separate values can be chained onto the command with spaces.

For example, "3y" will be recognized as 3 years, "10min" will be recognized as 10 minutes, and "1hr 30min" will be recognized as 90 minutes.

Time units can be expressed as their full name, or with the following shortcuts:

- **Minutes**: *mi*, *min*, *mins*
- **Hours**: *h*, *hr*, *hrs*
- **Days**: *d*
- **Months**: *mo*
- **Years**: *y*, *yr*, *yrs*

**Notes**:
- Seconds are not calculated.
- "m" is not implemented as a unit, because it can refer to either minutes or months.
- Months are added/subtracted using .NET's *DateTime.AddMonths()* method, which only flips the month and does not take the number of days into consideration (unless it is the end of the month, and the target month has fewer days than the current month).

### Time Difference

Rocketbox can calculate the amount of time between the current date/time and a given one.

**Syntax**: *[keyword] [date/time]*

**Keywords**: *since*, *until*, *ts*, *tu*

A date/time can be entered in virtually any format.

**Notes**:
- "since" and "until" can be used interchangeably, even if they contradict the target date.
- The number of months is not returned because of the ambiguous definition of a month.

### Convert To/From Unix Time

Rocketbox can convert a date/time into a Unix timestamp, or a Unix timestamp into a standard date/time.

**Syntax**: *[keyword] [value]*

**Keywords**:
- Converting ***to*** Unix time: *uto*
- Converting ***from*** Unix time: *ufrom*

## Google Translate shortcut

Syntax: *[keyword] [input language] [output language] [any number of words]*

**Keywords**: *translate*, *trans*, *tr*

Most languages on Google Translate are available and can be referenced by their full name, ISO two-letter language code, or various logical short forms.

## Closing Rocketbox background app

**Syntax**: *exit*

Rocketbox runs in the background, which is how it responds to the activation hotkey. Running this command will close it entirely.

## Custom Search Packs

Rocketbox can load lists of custom search engines from a plaintext file without requiring you to modify the database directly.

### Installing

**Syntax**: *install [file name]*

To install a search pack, a file with the extension .rbx must be placed in the same directory as Rocketbox.exe, and each line must be formatted as such:

```
name of search engine;;keywords;;search URL prefix
```

- *name of search engine* is the text that appears in Rocketbox when the command is typed.
- *keywords* is a comma-separated list of all of the possible keywords that can activate this search engine.
- *search URL prefix* is the URL that the search terms will be appended onto.
 
URLs that require content *after* the search terms are not supported at this time. Custom icons are not supported, but are planned for the future.

The file extension ".rbx" is implied by this command and does not need to be included.

### Uninstalling

**Syntax**: *uninstall [package name]*

This command will remove a previously installed package from the database, using the same name it was installed with.

### Viewing

**Syntax**: *packs*

This will create a file called "RocketboxPackages.txt" in the Rocketbox directory, populate it with a list of installed packages, and then open the file in Notepad.