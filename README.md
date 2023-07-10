[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql?label=InterpolatedSql)](https://www.nuget.org/packages/InterpolatedSql)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.svg)](https://www.nuget.org/packages/InterpolatedSql)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedSql.StrongName?label=InterpolatedSql.StrongName)](https://www.nuget.org/packages/InterpolatedSql.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedSql.StrongName.svg)](https://www.nuget.org/packages/InterpolatedSql.StrongName)

# Interpolated Sql Builder

**InterpolatedSqlBuilder is a dynamic SQL builder (but injection safe) where SqlParameters are defined using string interpolation.**

Parameters should just be embedded using interpolated objects, and they will be preserved (will not be mixed with the literals)
and will be parametrized when you need to run the command.

So it wraps the underlying SQL statement and the associated parameters, 
allowing to easily add new clauses to underlying statement and also add new parameters.


## Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/InterpolatedSql&type=Date)](https://star-history.com/#Drizin/InterpolatedSql&Date)


## License
MIT License
