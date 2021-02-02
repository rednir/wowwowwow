import sys
import gd

client = gd.Client()


async def Main():
    args = ''

    for a in sys.argv[1:]:
        if a == sys.argv[len(sys.argv) - 1]:
            args += a
        else:
            args += a + ' '

    # command: search
    if args.startswith('Daily'):
        await daily()
    elif args.startswith('Weekly'):
        await weekly()
    elif args.startswith('SEARCH_TERM'):
        await search(args[len('SEARCH_TERM') + 1:])


async def print_level_data(level):
    print(level.id)
    print(level.name)
    print(level.creator)
    print(level.difficulty)
    print(level.stars)
    print(level.downloads)
    print(level.rating)



#####    MAIN FUNCTIONS    ####

async def search(term):
    found_levels = await client.search_levels_on_page(0, term)
    found_levels = found_levels[:3]
    print(len(found_levels))
    for i in range(len(found_levels)):
        await print_level_data(found_levels[i])


async def daily():
    current_daily = await client.get_daily()
    print(1)
    await print_level_data(current_daily)


async def weekly():
    current_weekly = await client.get_weekly()
    print(1)
    await print_level_data(current_weekly)



####    CODE START   ####


try:
    client.run(Main())
except gd.errors.MissingAccess:
    print('NO_LEVELS')