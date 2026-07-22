import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';

export interface AppRadioOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface AppRadioProps {
  options: AppRadioOption[];
  value?: string;
  onValueChange?: (value: string) => void;
  name?: string;
  disabled?: boolean;
}

export function AppRadio({ options, value, onValueChange, name, disabled }: AppRadioProps) {
  return (
    <RadioGroup value={value} onValueChange={onValueChange} name={name} disabled={disabled}>
      {options.map((option) => (
        <div key={option.value} className="flex items-center gap-2">
          <RadioGroupItem
            value={option.value}
            id={`${name}-${option.value}`}
            disabled={option.disabled}
          />
          <Label htmlFor={`${name}-${option.value}`}>{option.label}</Label>
        </div>
      ))}
    </RadioGroup>
  );
}
