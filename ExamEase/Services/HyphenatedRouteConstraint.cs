namespace EMSYS.Services
{
    public class HyphenatedRouteConstraint : IRouteConstraint
    {
        public bool Match(
            HttpContext? httpContext,
            IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var routeValue))
            {
                var routeValueString = routeValue?.ToString();
                return !string.IsNullOrEmpty(routeValueString) && routeValueString.All(c => char.IsLetterOrDigit(c) || c == '-');
            }
            return false;
        }
    }
}
