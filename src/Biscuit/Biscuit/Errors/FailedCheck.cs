namespace Biscuit.Errors
{
    public class FailedCheck
    {
        public class FailedBlock : FailedCheck
        {
            public long BlockId;
            public long CaveatId;
            public string Rule;

            public FailedBlock(long blockId, long caveatId, string rule)
            {
                this.BlockId = blockId;
                this.CaveatId = caveatId;
                this.Rule = rule;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is FailedBlock)) return false;
                FailedBlock b = (FailedBlock)obj;
                return BlockId == b.BlockId && CaveatId == b.CaveatId && Rule.Equals(b.Rule);
            }

            public override int GetHashCode()
            {
                return Objects.Hash(BlockId, CaveatId, Rule);
            }

            public override string ToString()
            {
                return "FailedCaveat.FailedBlock { block_id: " + BlockId + ", caveat_id: " + CaveatId +
                   ", rule: " + Rule + " }";
            }
        }

        public class FailedVerifier : FailedCheck
        {
            public long caveat_id;
            public string rule;

            public FailedVerifier(long caveat_id, string rule)
            {
                this.caveat_id = caveat_id;
                this.rule = rule;
            }
            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is FailedVerifier)) return false;
                FailedVerifier b = (FailedVerifier)obj;
                return caveat_id == b.caveat_id && rule.Equals(b.rule);
            }

            public override int GetHashCode()
            {
                return Objects.Hash(caveat_id, rule);
            }

            public override string ToString()
            {
                return "FailedCaveat.FailedVerifier { caveat_id: " + caveat_id +
                        ", rule: " + rule + " }";
            }
        }
    }
}
