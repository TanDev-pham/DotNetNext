using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Auction> Auctions(bool isTracking = true) => (IQueryable<Auction>)(isTracking ? _context.Auctions : _context.Auctions.AsNoTracking());

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAll()
        {
            var auctions = await Auctions(isTracking: false).Include(auction => auction.Item)
                .OrderBy(auction => auction.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetById(Guid id)
        {
            var auction = await Auctions(isTracking: false).Include(auction => auction.Item)
                .FirstOrDefaultAsync(auction => auction.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> Create([FromBody] CreateAuctionDto auctionDto)
        {
            var createdAuction = _mapper.Map<Auction>(auctionDto);
            //TODO: add current user as seller
            createdAuction.Seller = "bob";


            await _context.AddAsync(createdAuction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not create auction");

            return CreatedAtAction(nameof(GetById),
                new { id = createdAuction.Id }, _mapper.Map<AuctionDto>(createdAuction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateAuctionDto auctionDto)
        {
            var auction = await Auctions().Include(auction => auction.Item)
                .FirstOrDefaultAsync(auction => auction.Id == id);

            if (auction == null) return NotFound();

            //TODO: check if user is seller

            if (auction.Status != Status.Live) return BadRequest("Cannot update auction");

            _mapper.Map(auctionDto, auction.Item);
            _context.Update(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not update auction");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var auction = await Auctions().FirstOrDefaultAsync(auction => auction.Id == id);

            if (auction == null) return NotFound();

            //TODO: check if user is seller

            if (auction.Status != Status.Live) return BadRequest("Cannot delete auction");

            _context.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not delete auction");

            return Ok();
        }
    }
}
