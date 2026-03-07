interface Props {
  status: string;
}

export default function StatusBadge({ status }: Props) {
  const className = `badge badge-${status.toLowerCase()}`;
  return <span className={className}>{status}</span>;
}
