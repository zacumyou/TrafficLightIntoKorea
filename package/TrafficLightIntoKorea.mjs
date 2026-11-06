/**
 * Cities: Skylines II UI Module
 * Id: TrafficLightIntoKorea
 * Author: zacum
 * Version: 2.3.69
 * Dependencies:
 */
// The existing DOM panel remains unchanged. Only the tooltip is rendered by CS2.
const React = window.React;
const {Tooltip} = window['cs2/ui'];

function NativeTooltipHost() {
  const [request, setRequest] = React.useState(null);
  React.useEffect(() => {
    const hide = () => setRequest(null);
    const bridge = {
      hide,
      show(target, text) {
        hide();
        if (!target || !text) return;
        if (document.body.contains(target)) setRequest({target, text});
      }
    };
    window.tlkNativeTooltip = bridge;
    window.addEventListener('blur', hide);
    window.addEventListener('resize', hide);
    return () => {
      window.removeEventListener('blur', hide);
      window.removeEventListener('resize', hide);
      if (window.tlkNativeTooltip === bridge) delete window.tlkNativeTooltip;
    };
  }, []);
  if (!request) return null;
  return React.createElement(Tooltip, {
    tooltip: request.text,
    anchorElRef: {current: request.target},
    forceVisible: true,
    hideOnInteraction: true
  }, React.createElement('span', {
    'aria-hidden': true,
    style: {position: 'fixed', width: 0, height: 0, pointerEvents: 'none'}
  }));
}

export default function register(moduleRegistry) {
  moduleRegistry.append('Game', NativeTooltipHost);
}
