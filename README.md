This solution has the following logic.
Does nothing if battery over 95%. (That was important)
Guesses how much battery each veicle consums per km and later calculates that exactly when first edge is traveled.
If a veicle has been charged two times and has enough battery to reach goal it does not go to a station. (No action)
Filters out stations that has no chargers or can't be reached with current battery level.
Does not go to the same station twice.
I tried to make a scheduler to see in the future if a station was free when arriving, but it didn't work.
Selects the closest station for stressed drivers but for all other driverers the closest green station.
I also tried to give a score to each station from all its parameters but didn't get a better score than above.
If I had had more time I´d add a fallback for when no station was found to do the same as above regardless of charge level.
I came in 3rd place.
