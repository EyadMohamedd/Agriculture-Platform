import Button from './Button';
import CardContent from './CardContent';
import Chip from './Chip';
import OutlinedInput from './OutlinedInput';
import TableCell from './TableCell';
import TableHead from './TableHead';
import TableRow from './TableRow';

export default function componentsOverride(theme) {
  return {
    ...Button(theme),
    ...CardContent(theme),
    ...Chip(theme),
    ...OutlinedInput(theme),
    ...TableCell(theme),
    ...TableHead(theme),
    ...TableRow(theme)
  };
}