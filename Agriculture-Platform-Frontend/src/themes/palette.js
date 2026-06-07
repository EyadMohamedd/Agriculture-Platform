import { presetPalettes } from '@ant-design/colors';
import ThemeOption from './theme';
import { extendPaletteWithChannels } from 'src/utils/colorUtils';

const greyAscent = ['#fafafa', '#bfbfbf', '#434343', '#1f1f1f'];

function buildGrey() {
  const greyPrimary = [
    '#ffffff','#fafafa','#f5f5f5','#f0f0f0','#d9d9d9',
    '#bfbfbf','#8c8c8c','#595959','#262626','#141414','#000000'
  ];
  const greyConstant = ['#fafafb', '#e6ebf1'];
  return [...greyPrimary, ...greyAscent, ...greyConstant];
}

export function buildPalette(presetColor) {
  const lightColors = { ...presetPalettes, grey: buildGrey() };
  const lightPaletteColor = ThemeOption(lightColors, presetColor);
  const commonColor = { common: { black: '#000', white: '#fff' } };
  const extendedLight = extendPaletteWithChannels(lightPaletteColor);
  const extendedCommon = extendPaletteWithChannels(commonColor);
  return {
    light: {
      mode: 'light',
      ...extendedCommon,
      ...extendedLight,
      text: {
        primary: extendedLight.grey[700],
        secondary: extendedLight.grey[500],
        disabled: extendedLight.grey[400]
      },
      action: { disabled: extendedLight.grey[300] },
      divider: extendedLight.grey[200],
      background: { paper: extendedLight.grey[0], default: extendedLight.grey.A50 }
    }
  };
}