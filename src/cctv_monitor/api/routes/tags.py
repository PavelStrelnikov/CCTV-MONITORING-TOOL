from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import TagCreate, TagOut, TagDefinitionUpdate
from cctv_monitor.storage.repositories import DeviceTagRepository, DeviceRepository

router = APIRouter(tags=["tags"])


@router.get("/tags", response_model=list[TagOut])
async def list_tags(session: AsyncSession = Depends(get_session)):
    tag_repo = DeviceTagRepository(session)
    defs = await tag_repo.get_all_tag_definitions()
    return [TagOut(name=d.name, color=d.color) for d in defs]


@router.post("/devices/{device_id}/tags", status_code=201)
async def add_tag(
    device_id: str,
    body: TagCreate,
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    tag_repo = DeviceTagRepository(session)
    try:
        await tag_repo.add_tag(device_id, body.tag)
        await session.commit()
    except IntegrityError:
        await session.rollback()
        raise HTTPException(status_code=409, detail="Tag already assigned to this device")
    return {"device_id": device_id, "tag": body.tag}


@router.delete("/devices/{device_id}/tags/{tag}", status_code=204)
async def remove_tag(
    device_id: str,
    tag: str,
    session: AsyncSession = Depends(get_session),
):
    tag_repo = DeviceTagRepository(session)
    removed = await tag_repo.remove_tag(device_id, tag)
    if not removed:
        raise HTTPException(status_code=404, detail="Tag not found")
    await session.commit()


@router.patch("/tags/{tag_name}", response_model=TagOut)
async def update_tag(
    tag_name: str,
    body: TagDefinitionUpdate,
    session: AsyncSession = Depends(get_session),
):
    tag_repo = DeviceTagRepository(session)
    updated = await tag_repo.update_tag_definition(
        tag_name, new_name=body.name, color=body.color,
    )
    if not updated:
        raise HTTPException(status_code=404, detail="Tag not found")
    await session.commit()
    return TagOut(name=updated.name, color=updated.color)


@router.delete("/tags/{tag_name}", status_code=204)
async def delete_tag(
    tag_name: str,
    session: AsyncSession = Depends(get_session),
):
    tag_repo = DeviceTagRepository(session)
    removed = await tag_repo.delete_tag_definition(tag_name)
    if not removed:
        raise HTTPException(status_code=404, detail="Tag not found")
    await session.commit()
